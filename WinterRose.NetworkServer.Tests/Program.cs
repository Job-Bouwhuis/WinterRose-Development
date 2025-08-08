using System.Diagnostics;
using System.Net;
using WinterRose.Networking;
using WinterRose.NetworkServer;
using WinterRose.NetworkServer.Connections;
using WinterRose.NetworkServer.Packets;
using WinterRose.WinterForgeSerializing;




try
{
    ServerConnection server = new(IPAddress.Any, 3000);
    server.Start();

    ClientConnection client = ClientConnection.Connect(IPAddress.Loopback, 3000, "Snow");
    //ClientConnection client2 = ClientConnection.Connect(IPAddress.Loopback, 3000, "Penguin");


    // RelayConnection? other =
    //     client.GetOtherConnectedClients()
    //         .Where(connection => connection.Identifier != client.Identifier)
    //         .FirstOrDefault();



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