using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ConsoleExtentions;
using WinterRose.Networking;
using WinterRose.Networking.TCP;

namespace WinterRose.FileServer
{
    internal class ServerMain
    {
        TCPServer server;
        public bool IsDisposed => server.IsDisposed;

        public ServerMain()
        {
            IPAddress address = Network.GetLocalIPAddress();

            //INPUT:
            //Console.Write("Enter port: ");
            //if(!int.TryParse(Console.ReadLine(), out int port))
            //{
            //    Console.WriteLine("Invalid port");
            //    goto INPUT;
            //}
            int port = 1200;

            server = new TCPServer();
            server.OnClientConnected += Server_OnClientConnected;
            server.OnClientDisconnected += Server_OnClientDisconnected;
            server.OnMessageReceived += Server_OnDataReceived;
            server.ResponseCommandReceived += Server_ResponseCommandReceived;
            server.OnError += Server_OnError;

            Windows.ApplicationExit += Windows_ApplicationExit;

            server.Start(address, port);
        }

        private void Server_OnError(ExceptionDispatchInfo info)
        {
            info.Throw();
        }

        private void Windows_ApplicationExit()
        {
            server.Dispose();
        }

        private void Server_ResponseCommandReceived(string command, TCPClientInfo sender, Guid responseID)
        {
            _ = ServerCommandHandling.HandleCommand(command, server, sender, responseID).ContinueWith(task => GC.Collect());
        }

        public void Close()
        {
            server.Dispose();
        }

        private void Server_OnDataReceived(string message, TCPClientInfo info1, TCPClientInfo? info2)
        {
            Console.WriteLine(message);
        }

        private void Server_OnClientDisconnected(TCPClientInfo info)
        {
            Console.WriteLine("client disconnected");
        }

        private void Server_OnClientConnected(TCPClientInfo info)
        {
            Console.WriteLine("Client connected");
        }
    }
}
