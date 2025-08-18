using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeVein.Connections;
public class ServerClientConnection : Connection
{
    private readonly TcpClient tcpClient;

    public Guid ClientId { get; }

    public string? Username { get; set; }
    public override NetworkStream Stream { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

    public override bool IsConnected => throw new NotImplementedException();

    public ServerClientConnection(TcpClient tcpClient)
    {
        ClientId = Guid.NewGuid();
        this.tcpClient = tcpClient;
    }

    // Override to handle incoming packets from client
    protected override async Task OnReceivePacketAsync(Packet packet)
    {
        // Process packet from client on server side
        // (e.g. validate, route, respond)
        await base.OnReceivePacketAsync(packet);
    }

    public async Task SendPacketAsync(NetworkPacket packet)
    {
        await SendAsync(packet);
    }

    public void Disconnect()
    {
        TcpClient.Close();
    }

    public override Task StartAsync(CancellationToken token = default)
    {

    }
    public override Task StopAsync() => throw new NotImplementedException();
    public override Stream GetStream() => throw new NotImplementedException();
    public override void Disconnect() => throw new NotImplementedException();
}

