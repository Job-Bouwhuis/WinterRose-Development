using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.ConsoleExtentions;
using WinterRose.NetworkServer;
using WinterRose.NetworkServer.Connections;
using WinterRose.NetworkServer.Packets;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Compiling;
using WinterRose.WinterThornScripting;

namespace WinterRose.NetworkServer.Connections;

public class ClientConnection : NetworkConnection
{
    private string domainUsed;
    private IPAddress ipAddressUsed;
    private TcpClient client;
    private NetworkStream stream;
    private Task listenerThread = null!;
    private bool tunnelImminent = false;

    public TunnelRequestReceivedHandler OnTunnelRequestReceived { get; } = new();
    public TunnelStream? ActiveTunnel { get; private set; }

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
        : base(logger is null ? new ConsoleLogger(nameof(ClientConnection), false) : logger)
    {
        IsServer = false;
        this.client = client;
        stream = client.GetStream();
        if (!isOnServerSide)
            Setup();
    }

    public static ClientConnection Connect(IPAddress ip, int port, string? username = null, ILogger? logger = null)
    {
        var client = new TcpClient();
        client.Connect(ip, port);

        var con = new ClientConnection(client, false, logger);
        con.ipAddressUsed = ip;
        while (!con.initialized) ;

        return con;
    }

    public static ClientConnection Connect(string hostname, int port, string? username = null, ILogger? logger = null)
    {
        IPAddress[] addresses = Dns.GetHostAddresses(hostname);

        var client = Connect(addresses.FirstOrDefault(), port, username, logger);
        client.domainUsed = hostname;
        return client;
    }

    private void Setup()
    {
        listenerThread = StartListeningForMessages();
    }

    public override void Send(Packet packet)
    {
        packet.SenderID = Identifier;
        packet.SenderUsername = Username;
        WinterForge.SerializeToStream(packet, stream);
        stream.Flush();
    }

    public override bool Send(Packet packet, Guid destination)
    {
        if (packet is RelayPacket relay)
        {
            if (relay.Content is RelayPacket.RelayContent relayContent)
            {
                relayContent.sender = Identifier;
                relayContent.destination = destination;
                relayContent.relayedPacket.SenderID = Identifier;
                relayContent.relayedPacket.SenderUsername = Username;
            }
        }
        else
        {
            packet.SenderID = Identifier;
            packet.SenderUsername = Username;
            packet = new RelayPacket(packet, Identifier, destination);
        }

        Send(packet);
        return true;
    }

    public override bool Disconnect()
    {
        try
        {
            if (ActiveTunnel != null && !ActiveTunnel.Closed)
            {
                ActiveTunnel.Close();
                ActiveTunnel = null;
            }

            Packet response = SendAndWaitForResponse(new DisconnectClientPacket(), timeout: TimeSpan.FromSeconds(5));

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
        return true;
    }

    private async Task StartListeningForMessages()
    {
        Task t = Task.Run(ListenForMessages);

        await t;

        if (t.Exception is not null)
        {
            throw t.Exception;
        }
    }

    private void ListenForMessages()
    {
        var serverSourceConnection = new RelayConnection(this);
        serverSourceConnection.SetSource(ConnectionSource.Server);

        {
            Packet packet;
            while ((packet = ReadPacket()) == null)
            {

            }

            if (packet is ConnectionCreatePacket ccp)
            {
                var content = ccp.Content as ConnectionCreatePacket.PContent;
                Identifier = content.guid;

                Send(new ClientHelloPacket(
                    domainUsed,
                    ipAddressUsed,
                    "1",
                    Assembly.GetEntryAssembly().GetName().Version.ToString(),
                    Username is "UNSET" ? null : Username));

                packet = ReadPacket();
                if (packet is not OkPacket)
                    throw new NotAllowedException("Server did not accept the client connection");
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
                if (tunnelImminent || ActiveTunnel is not null)
                {
                    if(ActiveTunnel is not null)
                    {
                        tunnelImminent = false;
                        if (ActiveTunnel.Closed)
                            ActiveTunnel = null;
                    }
                    continue; // let tunnel handle all the communication
                }

                Packet? packet = ReadPacket();
                if (packet is null)
                    continue;

                if (packet is TunnelAcceptedPacket)
                    tunnelImminent = true;
                if(packet is ReplyPacket rp)
                    if (((ReplyPacket.ReplyContent)rp.Content).OriginalPacket is TunnelAcceptedPacket)
                        tunnelImminent = true;

                if (packet is ServerStoppedPacket)
                {
                    logger.LogWarning("Server stopped");
                    break;
                }

                if (packet is RelayPacket relay)
                {
                    RelayPacket.RelayContent content = relay.Content as RelayPacket.RelayContent;
                    var sender = new RelayConnection(this)
                    {
                        RelayIdentifier = content.sender,
                    };
                    sender.SetSource(ConnectionSource.ClientRelay);
                    HandleReceivedPacket(content.relayedPacket, this, sender);
                }
                else
                    HandleReceivedPacket(packet, this, serverSourceConnection);
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error while listening for messages: " + ex.Message);
        }
    }

    private Packet ReadPacket()
    {
        while (true)
        {
            var data = ReadFromNetworkStream(stream);
            if(data is Nothing)
            {
                continue;
            }
            if (data is not Packet packet)
            {
                logger.LogError("Error: Data was not a valid packet.");
                continue;
            }
            return packet;
        }
    }

    internal TcpClient GetTcp() => client;
    internal void SetIdentifier(Guid identifier)
    {
        Identifier = identifier;
    }

    public override NetworkStream GetStream() => client.GetStream();

    /// <summary>
    /// Gets all the other connected clients from the server. this collection excludes this connection
    /// </summary>
    /// <returns></returns>
    public List<RelayConnection> GetOtherConnectedClients()
    {
        ConnectionListPacket? p = SendAndWaitForResponse(new ConnectionListPacket())
            as ConnectionListPacket;

        if (p == null)
            return [];

        return ((ConnectionListPacket.ConnectionListContent)p.Content)
            .connections.Where(id => id != Identifier).Select(id => new RelayConnection(this)
        {
            RelayIdentifier = id
        }).ToList();
    }

    public override bool TunnelRequestReceived(TunnelRequestPacket packet, NetworkConnection sender)
    {
        TunnelRequestPacket.TunnelReqContent? content = packet.Content as TunnelRequestPacket.TunnelReqContent;
        return OnTunnelRequestReceived.Invoke(content);
    }
    public override void TunnelRequestAccepted(Guid a, Guid b) 
    {
        TunnelStream tunnel = new(this);
        ActiveTunnel = tunnel;
    }

    public bool OpenTunnel(RelayConnection other)
    {
        Packet res = SendAndWaitForResponse(new TunnelRequestPacket(Identifier, other.RelayIdentifier));
        if (res is TunnelAcceptedPacket p)
        {
            TunnelRequestPacket.TunnelReqContent? content = p.Content as TunnelRequestPacket.TunnelReqContent;
            TunnelRequestAccepted(content.from, content.to);
            return true;
        }
        return false;
    }
}

internal class NotAllowedException : Exception
{
    public NotAllowedException(string message)
        :base(message)
    {
        
    }
}
