using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.NetworkServer.Packets;
using WinterRose.NetworkServer.Packets.Default.Packets;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterThornScripting;

namespace WinterRose.NetworkServer;

public class ServerConnection : NetworkConnection
{
    private readonly TcpListener serverListener;
    private readonly List<ClientConnection> clients;
    private readonly List<Task> clientTasks;
    private readonly CancellationTokenSource cancellationTokenSource;
    private Task listenTask;

    public event Action<ClientConnection> OnClientConnected = delegate { };

    public ServerConnection(IPAddress ip, int port)
    {
        serverListener = new TcpListener(ip, port);
        clients = [];
        clientTasks = [];
        cancellationTokenSource = new CancellationTokenSource();
        IsServer = true;
    }

    public ServerConnection(string ip, int port) : this(IPAddress.Parse(ip), port) { }

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
            ClientConnection cc = new(client, true);
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
        try
        {
            using (NetworkStream stream = tcp.GetStream())
            {
                while (tcp.Connected)
                {
                    object data = WinterForge.DeserializeFromStream(stream);
                    if (data is Nothing)
                        Console.WriteLine($"Client '{client.Identifier}' disconnected abruptly");
                    if (data is not Packet packet)
                    {
                        Console.WriteLine("Error: Data was not a valid packet.");
                        continue;
                    }
                    if(data is RelayPacket relay)
                    {
                        RelayPacket.RelayContent content = relay.Content as RelayPacket.RelayContent;

                        Send(data as Packet, content.destination);
                        return;
                    }

                    HandleReceivedPacket(packet, this, client);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error with client {client.Identifier}: {ex.Message}");
        }
        finally
        {
            clients.Remove(client);
            tcp.Close();
        }
    }

    public override void Send(Packet packet)
    {
        foreach (var client in clients)
            WinterForge.SerializeToStream(packet, client.GetTcp().GetStream());
    }

    public override void Send(Packet packet, Guid destination)
    {
        foreach (var client in clients)
            if(client.Identifier == destination)
            {
                client.Send(packet);
                return;
            }
    }

    public void Stop()
    {
        cancellationTokenSource.Cancel();
        serverListener.Stop();
    }

    public ClientConnection? GetClient(Guid identifier)
    {
        foreach (var client in clients)
            if (client.Identifier == identifier)
                return client;
        return null;
    }
}
