using System.Net;
using WinterRose.Networking;
using WinterRose.NetworkServer.Packets;
using WinterRose.NetworkServer.Packets.Default.Packets;

namespace WinterRose.NetworkServer.Tests;

internal class Program
{
    static void Main(string[] args)
    {
        ServerConnection server = new ServerConnection(Network.GetLocalIPAddress(), 12345);
        server.Start();
        Console.WriteLine("Server started on port 12345. Press [Enter] to stop the server...");
        ClientConnection client = ClientConnection.Connect(Network.GetLocalIPAddress(), 12345);
        client.SetUsername("TheSnowOwl"); // username is set on both the client and server. but only when the server responds with OkPacket

        Console.WriteLine(client.Ping());

        FilePacket.FileContent p = client.SendAndWaitForResponse(new DownloadFilePacket("WinterRose.xml")).Content as FilePacket.FileContent;
        p.Write("Test.xml");
        Console.ReadLine();

        server.Stop();
        Console.WriteLine("Server stopped.");
    }
}
