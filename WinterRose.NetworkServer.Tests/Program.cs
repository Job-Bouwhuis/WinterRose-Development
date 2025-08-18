using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using WinterRose;
using WinterRose.Networking;
using WinterRose.NetworkServer;
using WinterRose.NetworkServer.Connections;
using WinterRose.NetworkServer.Packets;
using WinterRose.WinterForgeSerializing;

try
{
    if (Debugger.IsAttached)
    {// 53802
        ServerConnection server = new(IPAddress.Any, 53802, clusterID: "c1");
        server.Username = "Server1";
        server.Start();
        server.logger.LogInformation("Server running on port {port}", server.Port);
        while (server.IsConnected) ;

        Windows.ApplicationExit += () =>
        {
            server.Disconnect();
            Console.WriteLine("Server1 stopped.");
        };
    }
    else
    {
        ServerConnection server = new(IPAddress.Any, 53803, clusterID: "c1");
        server.Username = "Server2";
        server.Start();
        server.logger.LogInformation("Server running on port {port}", server.Port);
        while (server.IsConnected) ;

        Windows.ApplicationExit += () =>
        {
            server.Disconnect();
            Console.WriteLine("Server2 stopped.");
        };
    }

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

// simple chat 'app'
/*
 var other = others[0];

        string message;

        while ((message = Console.ReadLine()) != "exit")
        {
            // Save the current cursor position so we can rewrite later
            int cursorTop = Console.CursorTop - 1;

            // Write the message in dark gray as it's being sent
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.SetCursorPosition(0, cursorTop);
            Console.WriteLine(message);

            Packet p = other.SendAndWaitForResponse(new Packet()
            {
                Header = new BasicHeader("Message"),
                Content = new Packets.StringContent(message)
            });

            // Rewrite message based on result
            Console.SetCursorPosition(0, cursorTop);

            if (p is OkPacket)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.WriteLine(message);

            // Reset color for next input
            Console.ResetColor();
        }
 */