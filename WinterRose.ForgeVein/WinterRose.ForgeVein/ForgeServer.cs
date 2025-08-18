using System.Net;
using System.Net.Sockets;
using WinterRose.ForgeVein.Connections;

namespace WinterRose.ForgeVein;

class ForgeServer
{
    private TcpListener listener;
    private List<Connection> clients = new();

    public event Action<Connection> ClientConnected;
    public event Action<Connection> ClientDisconnected;

    public ForgeServer(IPAddress ip, int port)
    {
        listener = new TcpListener(ip, port);
    }

    public void Start()
    {
        listener.Start();
        AcceptLoop();
    }

    private async void AcceptLoop()
    {
        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            var connection = new ServerClientConnection(client, this);
            clients.Add(connection);

            connection.StartListening();
            ClientConnected?.Invoke(connection);
        }
    }

    internal void RemoveClient(Connection connection)
    {
        clients.Remove(connection);
        ClientDisconnected?.Invoke(connection);
    }
}
