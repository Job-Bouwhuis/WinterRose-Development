using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinterRose.Networking.TCP;

/// <summary>
/// TCP Server that listens for incoming connections and delegates messages to and between clients.
/// </summary>
public class TCPServer(bool CreateConsoleOnConnect = true) : IClearDisposable
{
    /// <summary>
    /// Invoked when a message is received from a client. (only invoked for messages that are not requested to be sent to another client)
    /// </summary>
    public Action<string, TCPClientInfo, TCPClientInfo?> OnMessageReceived { get; set; } = delegate { };
    /// <summary>
    /// Invoked when a client uses the <see cref="SendAndResponseAsync(string, TcpClient, Guid?)"/> method and a response is received. and there is no target client. (meaning the message was meant for the server)
    /// </summary>
    public Action<string, TCPClientInfo, Guid> ResponseCommandReceived { get; set; } = delegate { };
    public Action<TCPClientInfo> OnClientConnected { get; set; } = delegate { };
    public Action<TCPClientInfo> OnClientDisconnected { get; set; } = delegate { };

    public TCPClientInfo[] ConnectedClients => [.. connections];

    private List<TCPClientInfo> connections = new();
    private TcpListener listener;
    private CancellationTokenSource cancelTokenSource = new();
    private Dictionary<Guid, Response<Packet>> pendingResponses = new();

    public bool IsDisposed { get; private set; }


    /// <summary>
    /// Connects the server to the given address and port, also starts the server.
    /// </summary>
    public void Start(string hostname, int port)
    {
        Start(IPAddress.Parse(hostname), port);
    }

    /// <summary>
    /// Connects the server to the given address and port, also starts the server.
    /// </summary>
    public void Start(IPAddress address, int port)
    {
        if (CreateConsoleOnConnect)
            Windows.OpenConsole();

        listener = new TcpListener(address, port);
        listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        listener.Start();
        ResponseCommandReceived += BuildInCommands;
        Console.WriteLine($"Server started on {listener.LocalEndpoint}");

        _ = Task.Run(() => AcceptClientsAsync(cancelTokenSource.Token));
    }

    /// <summary>
    /// Sends a message to the specified client.
    /// </summary>
    /// <param name="message"></param>
    public void Send(string message, TcpClient sender, Guid? targetClientId = null)
    {
        try
        {
            TCPClientInfo? targetClientInfo = null;

            if (targetClientId.HasValue)
            {
                targetClientInfo = connections.FirstOrDefault(c => c.Id == targetClientId.Value);
                if (targetClientInfo == null)
                {
                    Console.WriteLine("Target client not found.");
                    return;
                }
            }

            NetworkStream ns = (targetClientInfo?.Client ?? sender).GetStream();
            StreamWriter writer = new(ns);

            Packet packet;

            if(targetClientId.HasValue)
            {
                TCPClientInfo? info = connections.FirstOrDefault(x => x.Id == targetClientId.Value);
                if(info != null)
                {
                    packet = new(message, info, true);
                }
                else
                {
                    var col = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("INVALID USERID: " + targetClientId.Value);
                    Console.ForegroundColor = col;

                    packet = new(message);
                }

            }
            else
            {
                packet = new(message);
            }
            
            writer.WriteLine(packet.GetSerialized());

            writer.Flush();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Send Exception: {ex.Message}\n{ex.GetType().Name}");

            lock (connections)
            {
                connections.RemoveAll(c => c.Client == sender);
            }
        }
    }

    public async Task<Packet> SendAndResponseAsync(string message, TcpClient target, Guid? senderID = null)
    {
        var requestId = Guid.NewGuid();
        var tcs = new Response<Packet>();

        lock (pendingResponses)
        {
            pendingResponses[requestId] = tcs;
        }

        var payload = senderID.HasValue ? $"${senderID}:{requestId}:{message}" : $"${requestId}:{message}";

        Send(payload, target);

        Packet pack =  await tcs;  // Await the response

        lock(pendingResponses)
        {
            pendingResponses.Remove(requestId);
        }

        return pack;
    }

    public Packet SendAndResponse(string message, TcpClient target, Guid? senderID = null)
    {
        return SendAndResponseAsync(message, target, senderID).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    /// <param name="message"></param>
    public void Send(string message)
    {
        lock (connections)
        {
            foreach (var c in connections)
            {
                Send(message, c.Client, null);
            }
        }
    }

    /// <summary>
    /// Disposes the server and closes all connections. All connected clients will be notified of the disconnect.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed) return;

        IsDisposed = true;
        cancelTokenSource.Cancel();
        listener.Stop();

        lock (connections)
        {
            foreach (var client in connections)
            {
                client.Close();
            }
            connections.Clear();
        }
        Console.WriteLine("Server disposed.");

        GC.SuppressFinalize(this);
    }

    private async Task AcceptClientsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                if (client == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Client that tried to connect was null.");
                    Console.ResetColor();
                }

                Console.WriteLine("New client connecting...");
                TCPClientInfo clientInfo = new(client);
                Console.WriteLine($"Assigned GUID '{clientInfo.Id}' to this new client...");

                // Enable KeepAlive
                EnableKeepAlive(client, 60000, 10000);

                lock (connections)
                {
                    connections.Add(clientInfo);
                }
                Console.WriteLine("New Connection established.");

                _ = Task.Run(() => HandleClientAsync(client, cancellationToken));
                OnClientConnected.Invoke(clientInfo);
            }
        }
        catch (Exception ex) when (ex is ObjectDisposedException || ex is SocketException)
        {
            if (!IsDisposed)
                Console.WriteLine($"AcceptClientsAsync Exception: {ex.Message}");
        }
    }

    private void EnableKeepAlive(TcpClient client, int keepAliveTime, int keepAliveInterval)
    {
        var socket = client.Client;
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

        byte[] inArray = new byte[12];
        BitConverter.GetBytes((uint)1).CopyTo(inArray, 0);
        BitConverter.GetBytes((uint)keepAliveTime).CopyTo(inArray, 4);
        BitConverter.GetBytes((uint)keepAliveInterval).CopyTo(inArray, 8);

        socket.IOControl(IOControlCode.KeepAliveValues, inArray, null);
    }

    public void SendResponse(string response, TcpClient sender, Guid requestId)
    {
        var payload = $"~&&~{requestId}:{response}";

        lock (sender)
        {
            Send(payload, sender, null);
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        NetworkStream stream = client.GetStream();
        StreamReader reader = new(stream);
        StreamWriter writer = new(stream) { AutoFlush = true };

        TCPClientInfo clientInfo = connections.First(c => c.Client == client);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!client.Connected || clientInfo.ConnectionClosed)
                    break;

                string? serializedPacket = await reader.ReadLineAsync(cancellationToken);
                Packet packet = Packet.FromSerialized(serializedPacket);
                packet.Sender = clientInfo;
                if (packet.Payload == null)
                {
                    Console.WriteLine("Client disconnected.");
                    OnClientDisconnected.Invoke(clientInfo);
                    break;
                }

                _ = HandleMessage(packet, clientInfo);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HandleClientAsync Exception: {ex.Message}");
        }
        finally
        {
            writer.Dispose();
            reader.Dispose();
            client.Close();
        }
    }

    private async Task HandleMessage(Packet packet, TCPClientInfo sender)
    {
        string message = packet.Payload;
        bool expectResponse = message.StartsWith("$");
        bool isResponse = message.StartsWith("~&&~");
        message = message.TrimStart('$');
        if (message.StartsWith("~&&~"))
        {
            message = message[4..];
        }

        var parts = message.Split(':', 3);
        if ((expectResponse || isResponse) && Guid.TryParse(parts[0], out Guid requestId))
        {
            if (expectResponse)
            {
                ResponseCommandReceived?.Invoke(parts[1], sender, requestId);
                return;
            }
            var response = parts[1];
            Response<Packet>? tcs;
            lock (pendingResponses)
            {
                pendingResponses.TryGetValue(requestId, out tcs);
            }

            tcs?.SetResult(new Packet(response, sender, false));  // Set the result of the TaskCompletionSource
        }
        else
        {
            var targetClientId = parts.Length > 1 ? parts[0] : string.Empty;
            var payload = parts.Length > 1 ? parts[1] : string.Empty;

            if (payload == string.Empty) payload = parts[0];

            if (Guid.TryParse(targetClientId, out Guid targetId))
            {
                Send(payload, sender.Client, targetId);  // Forward the message
            }
            else
            {
                OnMessageReceived?.Invoke(payload, sender, null);
            }
        }
    }

    private void BuildInCommands(string message, TCPClientInfo sender, Guid requestID)
    {
        if (message is "^ping^")
        {
            SendResponse("Pong", sender.Client, requestID);
            return;
        }
        if (message is "^connectionlist^")
        {
            Console.WriteLine("A client asked for all connected clients.");

            var connectedGuids = connections.Select(c => c.Id.ToString());

            SendResponse(string.Join(",", connectedGuids), sender.Client, requestID);
            return;
        }

        if (message.StartsWith("^setusername^"))
        {
            var parts = message.Split('^', 3);
            var username = parts[2];

            Console.WriteLine($"Set client {sender.Id} from {sender.Username ?? "NONE"} to {username}");

            sender.SetName(username);
            SendResponse("OK", sender.Client, requestID);
        }

        if (message.StartsWith("^disconnect^"))
        {
            lock (connections)
            {
                Console.WriteLine($"Client {sender.Username} ({sender.Id}) disconnected gracefully.");
                SendResponse("OK", sender.Client, requestID);
                sender.Close();
                connections.Remove(sender);
            }
        }

        if (message.StartsWith("^myGUID^"))
        {
            SendResponse(sender.Id.ToString(), sender.Client, requestID);
        }

        if (message.StartsWith("^getclientinfo^"))
        {
            // ^getclientinfo^:targetGuid
            var parts = message.Split('^', 3, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length <= 1)
            {
                SendResponse("Invalid request.", sender.Client, requestID);
                return;
            }

            var targetId = Guid.Parse(parts.Last());

            var targetClient = connections.FirstOrDefault(c => c.Id == targetId);

            StringBuilder response = new();

            response.Append($"{targetClient.Id},{targetClient.Username},{targetClient.MachineName}");

            SendResponse(response.ToString(), sender.Client, requestID);
        }
    }
}
