using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using WinterRose;
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
    private readonly ILogger? logger;
    private NetworkStream stream;
    private Task listenerThread;

    private bool initialized = false;

    public bool SetUsername(string name)
    {
        Packet response = SendAndWaitForResponse(new SetUsernamePacket(name));
        if (response is not OkPacket)
            return false;
        Username = name;
        return true;
    }

    internal ClientConnection(TcpClient client, bool isOnServerSide, ILogger? logger)
    {
        IsServer = false;
        this.client = client;
        if (logger is null)
            logger = new ConsoleLogger(nameof(ClientConnection), false);
        else
            this.logger = logger;
        stream = client.GetStream();
        if (!isOnServerSide)
            Setup();
    }

    public static ClientConnection Connect(IPAddress ip, int port, ILogger? logger = null)
    {
        var client = new TcpClient();
        client.Connect(ip, port);

        var con = new ClientConnection(client, false, logger);

        while (!con.initialized) ;

        return con;
    }

    public static ClientConnection Connect(string ip, int port, ILogger? logger = null)
    {
        return Connect(IPAddress.Parse(ip), port, logger);
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

    public override bool Disconnect()
    {
        try
        {
            Packet response = SendAndWaitForResponse(new DisconnectClientPacket(), TimeSpan.FromSeconds(5));

            stream.Close();
            client.Close();

            if (response is OkPacket)
                return true;
            if (response is NoPacket)
                return false;
        }
        catch // assume already disconnected
        {
            try
            { // just to be sure, when an exception is raised
                stream.Close();
                client.Close();
            }
            catch
            {

            }
        }
        finally
        {
            
        }
        return true;
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
                logger.LogCritical("No handshake packet received before another");
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
            logger.LogCritical(ex, "Error while listening for messages: " + ex.Message);
        }
    }

    private Packet? ReadPacket()
    {
        var data = WinterForge.DeserializeFromStream(stream);
        if(data is Nothing)
        {
            logger.LogCritical("Server closed abruptly");
            return null;
        }
        if (data is not Packet packet)
        {
            logger.LogError("Error: Data was not a valid packet.");
            return null;
        }
        return packet;
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
