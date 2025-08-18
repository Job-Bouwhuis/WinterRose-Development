using System.Net.Sockets;

namespace WinterRose.ForgeVein.Connections;

class ForgeClient : Connection
{
    public override NetworkStream Stream { get; protected set; }

    public ForgeClient() { }

    public TcpClient TcpClient { get; private set; }

    public override bool IsConnected => TcpClient.Connected;

    public async Task ConnectAsync(string host, int port)
    {
        TcpClient = new TcpClient();
        await TcpClient.ConnectAsync(host, port);
        Stream = TcpClient.GetStream();
        StartListening();
    }

    public override void Disconnect()
    {
        TcpClient.Close();
    }
}
