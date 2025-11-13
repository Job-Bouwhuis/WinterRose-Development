using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.Recordium;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Instructions;

namespace WinterRose.NetworkServer.Connections;

public class ClusterDiscovery
{
    private static ConcurrentDictionary<Guid, Task> pendingConnections = [];

    public ServerConnection Server { get; }
    public string ClusterId { get; }
    public string Version { get; }
    public Guid NodeId { get; }
    public int UdpPort { get; }
    public int TcpPort { get; }
    public UdpClient UdpClient { get; }
    public IPEndPoint BroadcastEndpoint { get; }
    public Log? Logger { get; }
    private CancellationTokenSource cts = new CancellationTokenSource();

    public ClusterDiscovery(ServerConnection server, string clusterId, string version, int udpPort, int tcpPort)
    {
        this.Server = server;
        this.ClusterId = clusterId;
        this.Version = version;
        this.UdpPort = udpPort;
        this.TcpPort = tcpPort;
        this.NodeId = server.Identifier;
        Logger = new Log($"{server.logger.Category}_ClusterDC");

        // Setup UdpClient socket options before binding
        var udpClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        udpClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        udpClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
        udpClientSocket.EnableBroadcast = true;
        udpClientSocket.Bind(new IPEndPoint(IPAddress.Any, udpPort));
        this.UdpClient = new UdpClient { Client = udpClientSocket };

        BroadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, udpPort);

    }

    public void Start()
    {
        Task.Run(() => ListenForInvitesAsync(cts.Token));
        Task.Run(() => BroadcastHeartbeatsAsync(cts.Token));
    }

    public void Stop()
    {
        cts.Cancel();
        UdpClient.Dispose();
    }
    public async Task BroadcastHeartbeatsAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var invitation = new ClusterInvitation
                {
                    ClusterId = ClusterId,
                    Version = Version,
                    NodeId = NodeId,
                    NodeAddress = GetLocalIPAddress(),
                    TcpPort = TcpPort
                };

                using MemoryStream stream = new MemoryStream();
                WinterForge.SerializeToStream(invitation, stream);
                stream.Write([(byte)OpCode.END_OF_DATA]);
                byte[] data = stream.ToArray();

                try
                {
                    // Use the same udpClient for sending
                    await UdpClient.SendAsync(data, data.Length, BroadcastEndpoint);
                }
                catch (Exception ex)
                {
                    Logger?.Critical(ex, "[Broadcast Sender Error]");
                }

                await Task.Delay(5000, token);
            }
        }
        catch (Exception ex)
        {
            Logger?.Critical(ex, "[Broadcast Sender Error]");
        }
    }

    public async Task ListenForInvitesAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var result = await UdpClient.ReceiveAsync();
                    using MemoryStream stream = new MemoryStream(result.Buffer);

                    var invitation = WinterForge.DeserializeFromStream<ClusterInvitation>(stream);
                    if (invitation == null)
                        continue;

                    if (invitation.NodeId == NodeId || invitation.TcpPort == TcpPort)
                        continue;

                    if (invitation.ClusterId != ClusterId || invitation.Version != Version)
                        continue;

                    if (pendingConnections.ContainsKey(invitation.NodeId))
                        continue;

                    if (!IsConnectedTo(invitation.NodeId))
                    { 
                        int compare = NodeId.CompareTo(invitation.NodeId);

                        if(compare is not 0)
                            Logger?.Info($"[Discovered Node] {invitation.NodeAddress}:{invitation.TcpPort} (NodeId: {invitation.NodeId})");

                        if (compare < 0)
                        {
                            // This node has lower GUID - *act as client* - connect to invitation.NodeAddress:invitation.TcpPort
                            Task t = ConnectToNodeAsync(invitation.NodeAddress, invitation.TcpPort, invitation.NodeId);
                            pendingConnections.AddOrUpdate(NodeId, t, (g, t) => t);
                        }
                        else if (compare > 0)
                        {
                            // other client will send the join request
                        }
                        // 0 means same node that somehow got through, we ignore it
                    }
                }
                catch (EndOfStreamException) { }
                catch (Exception ex)
                {
                    Logger?.Critical(ex, "[Broadcast Listener Error]");
                }
            }
        }
        catch (Exception ex)
        {
            Logger?.Critical(ex, "[Broadcast Listener Error]");
        }
    }

    private async Task ConnectToNodeAsync(IPAddress nodeAddress, int tcpPort, Guid nodeId) => _ = Task.Run(() =>
    {
        try
        {
            Server.JoinCluster(nodeAddress, tcpPort);
        }
        catch (Exception ex)
        {
            Logger.Critical(ex, "Joining cluster failed");
        }

        pendingConnections.Remove(nodeId, out _);
    });
    private bool IsConnectedTo(Guid nodeId) => Server.GetClusterNode(nodeId) != null;

    private IPAddress GetLocalIPAddress()
    {
        string fallback = "127.0.0.1";
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip;
        }
        return IPAddress.Parse(fallback);
    }
}
