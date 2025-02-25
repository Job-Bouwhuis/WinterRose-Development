using System;
using System.Collections;
using System.Text;
using System.Net.Sockets;
using WinterRose.WIP.Redis.Utils;
using WinterRose.WIP.Redis.Exceptions;

namespace WinterRose.WIP.Redis.Framework
{

    public abstract class RedisConnectionBase : IClearDisposable
    {
        private Socket clientSocket = null;
        protected RedisStream redisStream = null;
        protected Queue sendCommanSyncQueue = null;
        protected bool isInTransaction = false;

        /// <summary>
        /// Event that is triggered when a progress report is available
        /// </summary>
        public event Action<ProgressReporter> ProgressReporter
        {
            add 
            { 
                redisStream.ProgressReporter += value;
                progressReporter += value;
            }
            remove
            {
                redisStream.ProgressReporter -= value;
                progressReporter -= value;
            }
        }

        protected event Action<ProgressReporter>? progressReporter;

        /// <summary>
        /// Returns if the current connection is linked to Redis
        /// </summary>
        protected bool IsConnected
        {
            get
            {
                return clientSocket.Connected;
            }
        }

        public bool IsDisposed { get; private set; }

        public RedisConnectionBase()
        {
            sendCommanSyncQueue = Queue.Synchronized(new Queue());
        }

        protected void CallProgressReporter(ProgressReporter progress)
        {
            progressReporter?.Invoke(progress);
        }

        protected void CreateConnection(string host, int port)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.SendTimeout = Constants.SOCKET_SEND_TIMEOUT_MS;
            clientSocket.ReceiveTimeout = Constants.SOCKET_RECEIVE_TIMEOUT_MS;
            clientSocket.Connect(host, port);
            redisStream = new RedisSynchronousStream(clientSocket);
        }

        protected string BuildCommand(RedisCommand command, params string[] args)
        {
            StringBuilder commandBuilder = new StringBuilder();
            commandBuilder.Append(Constants.NUM_ARGUMENTS);
            commandBuilder.Append(args.Length + 1).Append(Constants.CRLF);

            commandBuilder.Append(Constants.NUM_BYTES_ARGUMENT);
            commandBuilder.Append(command.ToString().Length).Append(Constants.CRLF);
            commandBuilder.Append(command.ToString()).Append(Constants.CRLF);

            foreach (string arg in args)
            {
                commandBuilder.Append(Constants.NUM_BYTES_ARGUMENT);
                commandBuilder.Append(arg.Length).Append(Constants.CRLF);
                commandBuilder.Append(arg).Append(Constants.CRLF);
            }
            return commandBuilder.ToString();
        }

        protected string[] ExecuteCommand(RedisCommand command, params string[] args)
        {
            try
            {
                while (sendCommanSyncQueue.Count != 0) ;
                byte[] bytes = PrepareBytesToSend(command, args);
                return redisStream.GetResponse(bytes) 
                    ?? throw new RedisException("Could not execute command > Response was null.");
            }
            catch (NullReferenceException)
            {
                throw new RedisException("Failed to execute command. no stream to redis established.");
            }
            catch (Exception e) when (e is not RedisException)
            {
                throw new RedisException("Failed to execute command.", e);
            }
        }

        protected void SendCommand(RedisCommand command, params string[] args)
        {
            byte[] bytes = PrepareBytesToSend(command, args);
            redisStream.SendData(bytes);
        }

        protected void EnqueueAndSendCommand(RedisCommand command, params string[] args)
        {
            sendCommanSyncQueue.Enqueue(command);
            SendCommand(command, args);
        }

        protected byte[] PrepareBytesToSend(RedisCommand command, string[] args)
        {
            string data = BuildCommand(command, args);
            return Encoding.UTF8.GetBytes(data);
        }

        protected bool ConvertReplyToBool(string[] reply)
        {
            if (isInTransaction) return false;
            string retVal = ((reply != null && reply.Length > 0) ? reply[0] : null);
            return ((retVal == null || retVal.Equals(Constants.NO_OP)) ? false : true);
        }

        protected int ConvertReplyToInt(string[] reply)
        {
            if (isInTransaction) return -1;
            string retVal = ((reply != null && reply.Length > 0) ? reply[0] : null);
            return ((retVal == null) ? -1 : Convert.ToInt32(retVal));
        }

        protected T ConvertReplyToEnum<T>(string enumValue) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), enumValue, true);
        }

        protected string ConvertReplyToString(string[] reply)
        {
            return ((reply != null && reply.Length > 0) ? reply[0] : null);
        }

        protected T ChangeStringToType<T>(string val)
        {
            return (T)Convert.ChangeType(val, typeof(T));
        }

        protected string ChangeTypeToString<T>(T val)
        {
            return (string)Convert.ChangeType(val, typeof(string));
        }

        protected string[] ChangeTypeArrayToString<T>(params T[] vals)
        {
            string[] args = new string[vals.Length];
            int index = 0;
            foreach (T val in vals)
                args[index++] = Convert.ToString(val);
            return args;
        }

        protected T[] ChangeStringArrayToType<T>(params string[] vals)
        {
            T[] args = new T[vals.Length];
            int index = 0;
            foreach (string val in vals)
                args[index++] = (T)Convert.ChangeType(val, typeof(T));
            return args;
        }

        protected void ValidateArguments<T>(params T[] args)
        {
            foreach (T argument in args)
                if (argument == null)
                    throw new ArgumentNullException();
        }

        protected void ValidateArguments(params string[] args)
        {
            foreach (string argument in args)
                if (argument == null)
                    throw new ArgumentNullException();
        }

        protected void Dispose()
        {
            if (IsDisposed) return;

            if (redisStream != null)
            {
                redisStream.CloseConnection();
                redisStream = null;
            }
            if (clientSocket != null)
            {
                clientSocket.Close();
                clientSocket = null;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose();
            IsDisposed = true;
        }
    }
}


