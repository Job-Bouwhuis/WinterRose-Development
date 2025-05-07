using System.Diagnostics;
using System.Net;
using System.Text;
using WinterRose.Networking;
using WinterRose.NetworkServer.Connections;
using WinterRose.NetworkServer.Packets;
using WinterRose.NetworkServer.Packets;

ClientConnection client = null;
ClientConnection client2 = null;
List<RelayConnection> others;
try
{
    char c = Console.ReadKey().KeyChar;

    var sw = Stopwatch.StartNew();
    while (sw.ElapsedMilliseconds < 1000) ;
    if (c == 'a')
    {
        Console.WriteLine(Network.GetLocalIPAddress());
        client = ClientConnection.Connect("192.168.2.15", 12345);
        client.SetUsername("TheSnowOwl");
        client.OnTunnelRequestReceived.Add(req => true);
        Console.WriteLine("enter on b connected");
        
        Console.ReadLine();

        others = client.GetOtherConnectedClients();
    }
    else
    {
        Console.WriteLine();
        client2 = ClientConnection.Connect("192.168.2.15", 12345);
        client2.SetUsername("TheSillyPenguin");
        client2.OnTunnelRequestReceived.Add(req => true);

        while(client2.ActiveTunnel == null)
        {
            
        }

        // client b
        var tunnelb = client2.ActiveTunnel!;
        StreamReader r = new(tunnelb);
        string s = r.ReadLine();
        Console.WriteLine(s);
        Console.ReadLine();
        return;
    }

    var other = others[0];

    if (client.OpenTunnel(other))
    {
        if (c == 'a')
        {
            // client a
            var tunnela = client.ActiveTunnel!;
            using StreamWriter wr = new(tunnela);
            Console.WriteLine("enter message");
            wr.WriteLine(Console.ReadLine());
            wr.Flush();
            tunnela.Close();
        }
    }
    else
    {
        Console.WriteLine("Failed");
    }
}
finally
{
    Console.ReadLine();
    client?.Disconnect();
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