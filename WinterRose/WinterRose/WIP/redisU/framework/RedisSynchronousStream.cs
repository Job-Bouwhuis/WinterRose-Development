using System.Text;
using System.Net.Sockets;
using WinterRose.WIP.Redis.Exceptions;
using WinterRose.WIP.Redis.Utils;
using System;
using System.Linq;
using System.Threading;
using System.Diagnostics.SymbolStore;

namespace WinterRose.WIP.Redis.Framework
{
    public sealed class RedisSynchronousStream : RedisStream
    {
        private NetworkStream networkStream;

        public int DownloadChunkSize { get; set; } = 1024 * 1024 * 10;  // 1MB
        public event Action<ProgressReporter> ProgressReporter;

        public RedisSynchronousStream(Socket socket)
        {
            networkStream = new(socket);
            networkStream.ReadTimeout = Constants.NS_READ_TIMEOUT_MS;
            networkStream.WriteTimeout = Constants.NS_WRITE_TIMEOUT_MS;
        }

        public string[] GetResponse(byte[] bytes)
        {
            Flush();
            SendData(bytes);
            return WaitAndParseReply();
        }

        public void SendData(byte[] bytes)
        {
            networkStream.Write(bytes, 0, bytes.Length);
        }

        private string[] WaitAndParseReply()
        {
            try
            {
                string replyCode = ReadSingleByte();
                string[] result = null;

                if (replyCode.Equals(Constants.REPLY_BULK))
                    result = new string[] { HandleBulkReply() };
                else if (replyCode.Equals(Constants.REPLY_MULTIBULK))
                    result = HandleMultiBulkReply();
                else if (replyCode.Equals(Constants.REPLY_ERROR))
                    HandleErrorReply();
                else if (replyCode.Equals(Constants.REPLY_INTEGER))
                    result = new string[] { HandleIntegerReply() };
                else if (replyCode.Equals(Constants.REPLY_SINGLE_LINE))
                    result = new string[] { HandleSingleLineReply() };

                return result;
            }
            catch (Exception e) when (e.Message.StartsWith("WRONGPASS"))
            {
                return [e.Message];
            }
            catch (Exception e)
            {
                throw new RedisException("Failed to parse reply.", e);
            }
        }

        public void HandleReply()
        {
            WaitAndParseReply();
        }

        public string? HandleBulkReply()
        {
            int dataLength = Convert.ToInt32(Readline());
            if (dataLength < 0 || !IsDataAvailable())
                return null;

            try
            {
                int kb11 = 1024 * 11;
                StringBuilder reply = new();

                int i = 0;
                int bytesLeft = dataLength;
                if (dataLength > kb11)
                {
                    int requests = 0;
                    int timesWaited = 0;
                    int maxWait = 1000;
                    while (bytesLeft != 0)
                    {
                        if(!IsDataAvailable())
                        {
                            timesWaited++;
                            if (timesWaited > maxWait)
                                throw new RedisException("Failed to download large key. Server did not respond in time. before finishing total download");
                            Thread.Sleep(1);
                            continue;
                        }
                        timesWaited = 0;
                        byte[] data;
                        if (bytesLeft > DownloadChunkSize)
                            data = new byte[DownloadChunkSize];
                        else
                            data = new byte[bytesLeft];

                        int readBytes = networkStream.Read(data, 0, data.Length);
                        bool b = data.Any(x => x is 0);
                        reply.Append(Encoding.UTF8.GetString(data)[..readBytes]);

                        float progress = (float)i / dataLength;
                        ProgressReporter?.Invoke(new ProgressReporter(progress * 100, $"handled {i} bytes of {dataLength}. Newly read bytes: {readBytes}"));
                        Thread.Sleep(30);
                        i += readBytes;
                        bytesLeft -= readBytes;
                        requests++;
                        //if (requests > 20)
                        //{
                        //    ProgressReporter?.Invoke(new ProgressReporter(progress * 100, $"Waiting for server..."));
                        //    Thread.Sleep(200);
                        //    requests = 0;
                        //}
                    }
                    return reply.ToString().TrimEnd('\0');
                }
            }
            catch (Exception e)
            {
                throw new RedisException("Fauled to download large key.", e);
            }


            {
                byte[] data = new byte[dataLength];
                try
                {
                    networkStream.Read(data, 0, data.Length);
                    ReadSingleByte(); //Read Carriage Return (\r)
                    ReadSingleByte(); //Read Line feed (\n)
                }
                catch (Exception)
                {
                    //Console.WriteLine("Network Stream read timeout: " + e.Message);
                    data = Array.Empty<byte>();
                }
                return Encoding.UTF8.GetString(data);
            }
        }

        public string[] HandleMultiBulkReply()
        {
            int numberOfBulkReplies = Convert.ToInt32(Readline());
            if (numberOfBulkReplies < 0)
                return null;
            string[] bulkReply = new string[numberOfBulkReplies];
            for (int index = 0; index < numberOfBulkReplies; index++)
            {
                ReadSingleByte();
                bulkReply[index] = HandleBulkReply();
            }
            if (IsDataAvailable())
                ReadSingleByte();
            return bulkReply;
        }

        public void HandleErrorReply()
        {
            String errorMessage = Readline();
            Console.WriteLine(errorMessage);
            throw new RedisException(errorMessage);
        }

        public string HandleIntegerReply()
        {
            return Readline();
        }

        public string HandleSingleLineReply()
        {
            return Readline();
        }

        public string[] HandleChannelMessage()
        {
            string[] channelMessageInfo = new string[3];
            for (int index = 0; index < channelMessageInfo.Length; index++)
            {
                string replyCode = ReadSingleByte();
                if (replyCode.Equals(string.Empty))
                    return null;
                if (replyCode.Equals(Constants.REPLY_BULK))
                    channelMessageInfo[index] = HandleBulkReply();
                else if (replyCode.Equals(Constants.REPLY_MULTIBULK))
                {
                    channelMessageInfo = HandleMultiBulkReply();
                    break;
                }
            }
            return channelMessageInfo;
        }

        public bool IsDataAvailable()
        {
            return networkStream.DataAvailable;
        }

        private string Readline()
        {
            StringBuilder builder = new StringBuilder();
            string temp = string.Empty;
            while (true)
            {
                temp = ReadSingleByte();

                if (temp.Equals("\r"))
                {
                    ReadSingleByte(); //Read \n
                    break;
                }
                else
                    builder.Append(temp);
            }
            return builder.ToString();
        }

        private string ReadSingleByte()
        {
            byte[] singleByteBuffer = new byte[1];
            try
            {
                networkStream.Read(singleByteBuffer, 0, singleByteBuffer.Length);
            }
            catch (Exception)
            {
                //Console.WriteLine("Network Stream read timeout: " + e.Message);
                singleByteBuffer = new byte[0];
            }
            return Encoding.UTF8.GetString(singleByteBuffer);
        }

        public void CloseConnection()
        {
            networkStream.Close();
            networkStream = null;
        }

        public void Flush()
        {
            while (networkStream.DataAvailable)
            {
                networkStream.ReadByte();
            }
        }
    }
}

