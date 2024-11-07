using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Networking.UDP
{
    /// <summary>
    /// A UDP server that can be used to send and receive messages.
    /// </summary>
    public sealed class UDPServer
    {
        /// <summary>
        /// the port this server runs on
        /// </summary>
        public readonly int Port;

        private Socket socket;
        private EndPoint endpoint;
        private byte[] bufferRecv;
        private ArraySegment<byte> bufferRecvSegment;

        /// <summary>
        /// Gets invoked when a new message is recieved.
        /// </summary>
        public Action<string, EndPoint> OnMessageRecieved = delegate { };

        /// <summary>
        /// Creates a new UDP server.
        /// </summary>
        /// <param name="port"></param>
        public UDPServer(int port)
        {
            Port = port;
            bufferRecv = new byte[4096];
            bufferRecvSegment = new(bufferRecv);

            endpoint = new IPEndPoint(IPAddress.Any, Port);

            socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
            socket.Bind(endpoint);
        }

        /// <summary>
        /// Starts the server
        /// </summary>
        public void StartServer()
        {
            _ = Task.Run(async () =>
            {
                SocketReceiveMessageFromResult res;
                while (true)
                {
                    res = await socket.ReceiveMessageFromAsync(bufferRecvSegment, SocketFlags.None, endpoint);
                    OnMessageRecieved(Encoding.UTF8.GetString(bufferRecv, 0, res.ReceivedBytes), res.RemoteEndPoint);
                }
            });
        }

        /// <summary>
        /// Sends bytes to the specified endpoint.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task SendAsync(string message, EndPoint endpoint)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await socket.SendToAsync(buffer, SocketFlags.None, endpoint);
        }
    }
}
