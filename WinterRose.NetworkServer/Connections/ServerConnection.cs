using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.ForgeGuardChecks;
using WinterRose.NetworkServer.Packets;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Compiling;
using WinterRose.WinterForgeSerializing.Formatting;

namespace WinterRose.NetworkServer.Connections;

public class ServerConnection : NetworkConnection
{
    private readonly TcpListener serverListener;
    private readonly List<ClientConnection> clients;
    private readonly List<ClusterClient> clusterClients = [];
    private readonly List<Task> clientTasks;
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly TunnelConnectionHandler tunnelConnectionHandler;
    private readonly ClusterDiscovery? clusterMapper;
    private Task listenTask;

    public int Port => ((IPEndPoint)serverListener.LocalEndpoint).Port;

    public override bool IsConnected => serverListener.Server.IsBound && !cancellationTokenSource.IsCancellationRequested;

    public event Action<ClientConnection> OnClientConnected = delegate { };

    public string LogPrefix { get; set; } = "WinterRoseServer: ";

    public ServerConnection(IPAddress ip, int port, ILogger? logger = null, string? clusterID = null)
        : base(logger is null ? new ConsoleLogger(nameof(ServerConnection), true) : logger)
    {
        serverListener = new TcpListener(ip, port);
        tunnelConnectionHandler = new();
        clients = [];
        clientTasks = [];
        cancellationTokenSource = new CancellationTokenSource();

        IsServer = true;
        Identifier = Guid.CreateVersion7();

        if (clusterID is not null)
        {
            base.logger?.LogInformation("Cluster broadcasting on port 55620");
            clusterMapper = new(this, clusterID, "1", 55620, port, base.logger);
            clusterMapper.Start();
        }
    }

    public ServerConnection(string ip, int port, ILogger? logger = null, string? clusterID = null) 
        : this(IPAddress.Parse(ip), port, logger, clusterID)
    {
    }

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
            Guid clientID = Guid.CreateVersion7();
            ClientConnection cc = new(client, true, new ConsoleLogger($"CLIENT ({clientID})"), null);
            cc.SetIdentifier(clientID);
            clients.Add(cc);

            clientTasks.Add(Task.Run(() => HandleClient(cc), cancellationToken));
        }
    }

    internal class ClientContext
    {
        public NetworkConnection Connection { get; set; }
        public ClientConnection Client => (Connection as ClientConnection)!;
        public ClusterClient ClusterNode => (Connection as ClusterClient)!;

        public bool IsClusterNode => Connection is ClusterClient;
        
        public static implicit operator ClientConnection(ClientContext context) => context.Client;
        public static implicit operator ClusterClient(ClientContext context) => context.ClusterNode;

        public static implicit operator NetworkConnection(ClientContext context) => context.Connection;

        public static implicit operator ClientContext(ClientConnection connection) => new() { Connection = connection };
        public static implicit operator ClientContext(ClusterClient connection) => new() { Connection = connection };
    }

    private void HandleClient(ClientConnection client)
    {
        TcpClient tcp = client.GetTcp();

        ConnectionCreatePacket ccp = new(client.Identifier);

        OnClientConnected(client);

        logger.LogInformation("New client connected on UUID: " + client.Identifier.ToString());

        ClientContext context = client;

        try
        {
            
            using (NetworkStream stream = tcp.GetStream())
            {
                client.Send(ccp);
                object first = ReadFromNetworkStream(stream);
                if (first is ClientHelloPacket clientHello)
                {
                    var helloData = (ClientHelloPacket.ClientHelloContent)clientHello.Content;
                    logger.LogInformation($"Connected: {helloData.Username}");
                    client.Send(new OkPacket());
                }

                while (client.IsConnected)
                {
                    if (tunnelConnectionHandler.InTunnel(client))
                        continue; // skip loading packets while client is in a tunnel.
                    object data = ReadFromNetworkStream(stream);
                    bool? shouldContinue = PrehandlePacket(context, data, context);
                    if (shouldContinue == false)
                        break;
                    else if (shouldContinue == true)
                        continue;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogPrefix + $"Error with client {client.Identifier}: {ex.Message}", client);
        }
        finally
        {
            clients.Remove(context);
            clusterClients.Remove(context);
            tcp.Close();
        }
    }

    internal bool? PrehandlePacket(ClientConnection client, object data, ClientContext context)
    {
        if (data is Nothing)
        {
            if (client.Username != "UNSET")
                logger.LogWarning(LogPrefix + $"Client '{client.Identifier}' ({client.Username}) disconnected abruptly");
            else
                logger.LogWarning(LogPrefix + $"Client '{client.Identifier}' disconnected abruptly");
            return true;
        }

        if (data is not Packet packet)
        {
            logger.LogError("Error: Data was not a valid packet.");
            return true;
        }

        if (packet is DisconnectClientPacket disconnect)
        {
            if (client.Username != "UNSET")
                logger.LogInformation(LogPrefix + $"Client '{client.Identifier}' ({client.Username}) disconnected gracefully");
            else
                logger.LogInformation(LogPrefix + $"Client '{client.Identifier}' disconnected gracefully");
            return false;
        }

        if (packet.Content is ReplyPacket.ReplyContent reply)
        {
            if (reply.OriginalPacket is DisconnectClientPacket)
            {
                client.Send(((ReplyPacket)packet).Reply(new OkPacket(), this));

                if (client.Username != "UNSET")
                    logger.LogInformation(LogPrefix + $"Client '{client.Identifier}' ({client.Username}) disconnected gracefully");
                else
                    logger.LogInformation(LogPrefix + $"Client '{client.Identifier}' disconnected gracefully");
                return false;
            }

            if (reply.OriginalPacket is ClusterHelloPacket hello)
            {
                var helloData = (ClusterHelloPacket.ClusterHelloContent)hello.Content;
                logger.LogInformation(
                    $"[ClusterNode] Node connected: ID={helloData.NodeId}, Cluster={helloData.ClusterId}, Version={helloData.Version}");
                clients.Remove(client);
                client.IsServer = true;
                context.Connection = new ClusterClient(this, client, logger);
                clusterClients.Add(context.ClusterNode);
                client.Identifier = helloData.NodeId;

                client.Send(((ReplyPacket)packet).Reply(
                    new ClusterHelloPacket(Identifier, clusterMapper.ClusterId, clusterMapper.Version), this));

                logger.LogInformation("Connected to cluster member {id}", helloData.NodeId);

                return true;
            }
        }

        if (packet is RelayPacket && packet.Content is RelayPacket.RelayContent rc)
        {
            if (rc.destination == Guid.Empty)
                packet = rc.relayedPacket; // packet was meant for the server
        }

        //_ = Task.Run(() => HandleReceivedPacket(packet, this, client));
        HandleReceivedPacket(packet, this, client);
        return null;
    }

    public override void Send(Packet packet)
    {
        packet.SenderID = Identifier;
        packet.SenderUsername = Username;

        foreach (var client in clients)
        {
            using var stream = client.GetTcp().GetStream();
            WinterForge.SerializeToStream(packet, stream/*, TargetFormat.IntermediateRepresentation*/);
            stream.Flush();
        }
    }

    public override bool Send(Packet packet, Guid destination)
    {
        foreach (var client in clients)
            if (client.Identifier == destination)
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

    public ClusterClient? GetClusterNode(Guid identifier)
    {
        for (int i = 0; i < clusterClients.Count; i++)
        {
            ClusterClient? client = clusterClients[i];
            if (!client.IsConnected)
            {
                clusterClients.Remove(client);
                i--;
                continue;
            }
            if (client.Identifier == identifier)
                return client;
        }
        return null;
    }

    /// <summary>
    /// Stops the server, and closes all client connections gracefully.
    /// </summary>
    /// <returns></returns>
    public override bool Disconnect()
    {
        foreach (var client in clients)
            client.Send(new ServerStoppedPacket());
        return true;
    }

    /// <summary>
    /// Stops the server, and closes all client connections gracefully.
    /// </summary>
    /// <returns></returns>
    public bool Stop() => Disconnect();

    /// <summary>
    /// Stops the server immediately, without gracefully signalling clients that the server is stopping.
    /// <br></br>This will close all client connections immediately.
    /// </summary>
    public void StopImmediately()
    {
        serverListener.Stop();
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

    public override void TunnelRequestAccepted(Guid a, Guid b)
    {
    }

    internal void JoinCluster(IPAddress nodeAddress, int tcpPort)
    {
        Forge.Expect(clusterMapper).Not.Null();
        ClusterHelloPacket hello = new(Identifier, clusterMapper.ClusterId, clusterMapper.Version);
        var client = ClientConnection.Connect(nodeAddress, tcpPort, true);
        clusterClients.Add(new ClusterClient(this, client, logger));

        ClusterHelloPacket resp = client.SendAndWaitForResponse(hello) as ClusterHelloPacket;
        var helloData = (ClusterHelloPacket.ClusterHelloContent)resp.Content;
        client.Identifier = helloData.NodeId;

        logger.LogInformation("Connected to cluster member {id}", helloData.NodeId);
    }

    internal void RemoveClusterClient(ClusterClient clusterClient)
    {
        clusterClients.Remove(clusterClient);
    }
}