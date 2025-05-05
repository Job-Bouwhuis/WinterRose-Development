using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.ConsoleExtentions;
using WinterRose.NetworkServer;
using WinterRose.NetworkServer.Connections;
using WinterRose.NetworkServer.Packets;
using WinterRose.NetworkServer.Packets.Default.Packets;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterThornScripting;

public class ClientConnection : NetworkConnection
{
    private TcpClient client;
    private NetworkStream stream;
    private Task listenerThread;

    private bool initialized = false;

    public void SetUsername(string name)
    {
        Packet response = SendAndWaitForResponse(new SetUsernamePacket(name));
        if (response is not OkPacket)
            throw new Exception("Username not accepted!");
        Username = name;
    }

    internal ClientConnection(TcpClient client, bool isOnServerSide)
    {
        IsServer = false;
        this.client = client;
        stream = client.GetStream();
        if (!isOnServerSide)
            Setup();
    }

    public static ClientConnection Connect(IPAddress ip, int port)
    {
        var client = new TcpClient();
        client.Connect(ip, port);

        var con = new ClientConnection(client, false);

        while (!con.initialized) ;

        return con;
    }

    public static ClientConnection Connect(string ip, int port)
    {
        return Connect(IPAddress.Parse(ip), port);
    }

    private void Setup()
    {
        listenerThread = StartListeningForMessages();
    }

    public override void Send(Packet packet)
    {
        //string s = WinterForge.SerializeToString(packet);

        WinterForge.SerializeToStream(packet, stream);
    }

    public override void Send(Packet packet, Guid destination)
    {

    }

    private async Task StartListeningForMessages()
    {
        Task t = Task.Run(ListenForMessages);

        await t;

        if (t.Exception is not null)
        {

        }
    }

    private void ListenForMessages()
    {
        var serverSourceConnection = new RelayConnection(this);
        serverSourceConnection.SetSource(ConnectionSource.Server);

        {
            Packet packet = ReadPacket();

            if (packet is ConnectionCreatePacket ccp)
            {
                var content = ccp.Content as ConnectionCreatePacket.PContent;
                Identifier = content.guid;
                initialized = true;
            }
            else
            {
                Console.WriteLine("No handshake packet received before another");
                HandleReceivedPacket(packet, this, serverSourceConnection);
            }
        }

        try
        {
            while (client.Connected)
            {
                Packet? packet = ReadPacket();
                if (packet is null)
                    continue;

                HandleReceivedPacket(packet, this, serverSourceConnection);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while listening for messages: " + ex.Message);
        }
    }

    private Packet? ReadPacket()
    {
        var data = WinterForge.DeserializeFromStream(stream);
        if(data is Nothing)
            Console.WriteLine("Server closed abruptly");
        if (data is not Packet packet)
        {
            Console.WriteLine("Error: Data was not a valid packet.");
            return null;
        }
        return packet;
    }

    public void Disconnect()
    {
        stream.Close();
        client.Close();
    }

    internal TcpClient GetTcp() => client;
    internal void SetIdentifier(Guid identifier)
    {
        Identifier = identifier;
    }

    public TimeSpan Ping()
    {
        DateTime now = DateTime.UtcNow;
        PongPacket pong = (PongPacket)SendAndWaitForResponse(new PingPacket());
        DateTime roundTrip = new DateTime(((PingPacket.PingContent)pong.Content).timestamp);
        return roundTrip - now;
    }
}
