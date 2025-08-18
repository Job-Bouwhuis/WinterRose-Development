using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinterRose.NetworkServer.Packets;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.NetworkServer.Connections;
public class ClusterClient : NetworkConnection
{
    private ServerConnection server;
    private ClientConnection client;

    public override bool IsConnected => server.IsConnected && client.IsConnected;
    public override Guid Identifier { get => client.Identifier; internal set => client.Identifier = value; }

    public ClusterClient(ServerConnection server, ClientConnection serverSideClient, ILogger? logger = null)
        : base(logger ?? new ConsoleLogger(nameof(ClusterClient), true))
    {
        this.server = server;
        client = serverSideClient;
        IsServer = true;
    }

    public override bool Disconnect()
    {
        server.RemoveClusterClient(this);
        return true;
    }
    public override NetworkStream GetStream() => server.GetStream();
    public override void Send(Packet packet) => server.Send(packet);
    public override bool Send(Packet packet, Guid destination) => server.Send(packet, destination);
    public override void TunnelRequestAccepted(Guid a, Guid b) { }
    public override bool TunnelRequestReceived(TunnelRequestPacket packet, NetworkConnection sender) => false;


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

        ServerConnection.ClientContext context = this;

        try
        {
            while (server.IsConnected)
            {
                Packet? packet = ReadPacket();
                if (packet is null)
                    continue;

                if (packet is ReplyPacket rp)
                {
                    if (((ReplyPacket.ReplyContent)rp.Content).OriginalPacket is DisconnectClientPacket)
                    {
                        logger.LogInformation("Cluster client {id} closed and thereby left the cluster", Identifier);
                        Disconnect();
                        break;
                    }
                }

                if (packet is DisconnectClientPacket)
                {
                    logger.LogInformation("Cluster client {id} closed and thereby left the cluster", Identifier);
                    Disconnect();
                    break;
                }

                server.PrehandlePacket(client, packet, context);
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
            var data = ReadFromNetworkStream(client.stream);
            if (data is Nothing)
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
}
