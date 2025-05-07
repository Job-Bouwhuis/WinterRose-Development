using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.NetworkServer.Connections;
using WinterRose.NetworkServer.Packets;
using WinterRose.NetworkServer.Packets;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.NetworkServer;

public class ServerConnection : NetworkConnection
{
    private readonly TcpListener serverListener;
    private readonly List<ClientConnection> clients;
    private readonly List<Task> clientTasks;
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly TunnelConnectionHandler tunnelConnectionHandler;
    private Task listenTask;

    public event Action<ClientConnection> OnClientConnected = delegate { };

    public string LogPrefix { get; set; } = "WinterRoseServer: ";

    public ServerConnection(IPAddress ip, int port, ILogger? logger = null)
        : base(logger is null ? new ConsoleLogger(nameof(ClientConnection), false) : logger)
    {
        serverListener = new TcpListener(ip, port);
        tunnelConnectionHandler = new();
        clients = [];
        clientTasks = [];
        cancellationTokenSource = new CancellationTokenSource();
        IsServer = true;
    }

    public ServerConnection(string ip, int port, ILogger? logger = null) : this(IPAddress.Parse(ip), port, logger) { }

    public void Start(CancellationToken cancellationToken)
    {
        serverListener.Start();
        ListenForClientsAsync(cancellationToken);
    }

    public void Start()
    {
        serverListener.Start();
        listenTask = ListenForClientsAsync(cancellationTokenSource.Token);
    }

    private async Task ListenForClientsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            TcpClient client = await serverListener.AcceptTcpClientAsync(cancellationToken);
            ClientConnection cc = new(client, true, null);
            cc.SetIdentifier(Guid.NewGuid());
            clients.Add(cc);

            clientTasks.Add(Task.Run(() => HandleClient(cc), cancellationToken));
        }
    }

    private void HandleClient(ClientConnection client)
    {
        TcpClient tcp = client.GetTcp();

        ConnectionCreatePacket ccp = new(client.Identifier);
        client.Send(ccp); // handshake packet

        OnClientConnected(client);

        logger.LogInformation("New client connected on UUID: " + client.Identifier.ToString());

        try
        {
            using (NetworkStream stream = tcp.GetStream())
            {
                while (tcp.Connected)
                {
                    if (tunnelConnectionHandler.InTunnel(client))
                        continue; // skip loading packets while client is in a tunnel.
                    object data = WinterForge.DeserializeFromStream(stream);
                    if (data is Nothing)
                        logger.LogWarning(LogPrefix + $"Client '{client.Identifier}' disconnected abruptly");
                    if (data is not Packet packet)
                    {
                        logger.LogError("Error: Data was not a valid packet.");
                        continue;
                    }
                    if(packet is DisconnectClientPacket disconnect)
                    {
                        logger.LogInformation(LogPrefix + "Client gracefully disconnected: " + client.Identifier, client);
                        break;
                    }
                    if(packet.Content is ReplyPacket.ReplyContent reply && reply.OriginalPacket is DisconnectClientPacket)
                    {
                        client.Send(((ReplyPacket)packet).Reply(new OkPacket(), this));
                        logger.LogInformation(LogPrefix + "Client gracefully disconnected: " + client.Identifier, client);
                        break;
                    }
                    if(packet is RelayPacket && packet.Content is RelayPacket.RelayContent rc)
                    {
                        if (rc.destination == Guid.Empty)
                            packet = rc.relayedPacket; // packet was meant for the server
                    }

                    //_ = Task.Run(() => HandleReceivedPacket(packet, this, client));
                    HandleReceivedPacket(packet, this, client);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogPrefix + $"Error with client {client.Identifier}: {ex.Message}", client);
        }
        finally
        {
            clients.Remove(client);
            tcp.Close();
        }
    }

    public override void Send(Packet packet)
    {
        packet.SenderID = Guid.Empty;
        packet.SenderUsername = "SERVER";

        foreach (var client in clients)
            WinterForge.SerializeToStream(packet, client.GetTcp().GetStream());
    }

    public override bool Send(Packet packet, Guid destination)
    {
        foreach (var client in clients)
            if(client.Identifier == destination)
            {
                client.Send(packet);
                return true;
            }

        return false;
    }

    public ClientConnection? GetClient(Guid identifier)
    {
        foreach (var client in clients)
            if (client.Identifier == identifier)
                return client;
        return null;
    }

    public override bool Disconnect()
    {
        foreach (var client in clients)
            client.Send(new ServerStoppedPacket());
        return true;
    }

    public override NetworkStream GetStream() => null!; // Server doesnt open a stream by itself
    internal List<ClientConnection> GetClients() => clients;
    public override bool TunnelRequestReceived(TunnelRequestPacket packet, NetworkConnection sender)
    {
        TunnelRequestPacket.TunnelReqContent? content = packet.Content as TunnelRequestPacket.TunnelReqContent;
        Packet response = SendAndWaitForResponse(packet, content.to);
        if (response is TunnelAcceptedPacket)
        {
            var a = GetClient(content.from);
            var b = GetClient(content.to);
            tunnelConnectionHandler.DefineTunnel(a, b);

            return true;
        }
        else
            return false;
    }
    public override void TunnelRequestAccepted(Guid a, Guid b) { }
}
