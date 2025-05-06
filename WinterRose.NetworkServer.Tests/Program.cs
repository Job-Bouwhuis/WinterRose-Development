using System.Net;
using WinterRose.Networking;
using WinterRose.NetworkServer.Packets;
using WinterRose.NetworkServer.Packets.Default.Packets;

namespace WinterRose.NetworkServer.Tests;

internal class Program
{
    static void Main(string[] args)
    {
        ClientConnection client = ClientConnection.Connect(Network.GetLocalIPAddress(), 12345);
        var others = client.GetOtherConnectedClients();
        if (others.Count == 0)
        {
            client.SetUsername("TheSnowOwl");
            Console.WriteLine("Waiting for other...");
            Console.ReadLine();
            others = client.GetOtherConnectedClients();
        }
        else
        {
            client.SetUsername("TheSillyPenguin");

            if (others.Count == 1)
            {
                var o = others[0];
                string username = o.Username;
                Console.WriteLine("me: " + client.Username + "\nother: " + username);
            }
            else
                Console.WriteLine("I was the only connection.");
        }

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


        Console.ReadLine();
        client.Disconnect();
    }
}
