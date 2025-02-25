using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Networking.UDP
{
    public sealed class UDPClient : IClearDisposable
    {
        private Socket socket;
        private EndPoint endpoint;
        private byte[] bufferRecv;
        private ArraySegment<byte> bufferRecvSegment;

        /// <summary>
        /// Gets invoked when a message has been recieved from the server
        /// </summary>
        public Action<string> OnServerMessageRecieved;

        public bool IsDisposed { get; private set; }

        public UDPClient(IPAddress address, int port)
        {
            bufferRecv = new byte[4096];
            bufferRecvSegment = new(bufferRecv);

            endpoint = new IPEndPoint(address, port);

            socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
        }

        /// <summary>
        /// Makes it this client listenes for any messages from the server
        /// </summary>
        public void StartListening()
        {
            _ = Task.Run(async () =>
            {
                SocketReceiveMessageFromResult res;
                while (true)
                {
                    res = await socket.ReceiveMessageFromAsync(bufferRecvSegment, SocketFlags.None, endpoint);
                    string message = Encoding.UTF8.GetString(bufferRecv, 0, res.ReceivedBytes);
                    OnServerMessageRecieved(message);
                    Console.WriteLine(message);

                }
            });
        }

        public async Task SendAsync(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await socket.SendToAsync(buffer, SocketFlags.None, endpoint);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            socket.Dispose();
        }
    }
}
