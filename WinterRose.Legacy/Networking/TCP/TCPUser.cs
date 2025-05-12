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
/// TCP Client that connects to a server and sends and receives messages.
/// </summary>
public class TCPUser(bool CreateConsoleOnConnect = true) : IClearDisposable
{
    /// <summary>
    /// Invoked when a message is received from the server.
    /// <br></br>
    /// 2nd parameter is this client's information.
    /// <br></br>
    /// 3rd parameter is a possible client ID that has requested the message to be sent to this client. May be null if the message was sent by the server.
    /// </summary>
    public Action<string, TCPUser, TCPClientInfo?> OnMessageReceived { get; set; } = delegate { };
    /// <summary>
    /// Invoked when the server has sent a message to this client that expects a response. Second parameter is the request ID.
    /// </summary>
    public Action<string, Guid> ResponseMessageReceived { get; set; } = delegate { };
    /// <summary>
    /// Invoked when the server has shutdown.
    /// </summary>
    public Action OnServerShutdown { get; set; }
    /// <summary>
    /// Invoked when the server has closed the connection.
    /// </summary>
    public Action OnServerClosed { get; set; } = delegate { };
    public bool IsDisposed { get; private set; }
    public bool IsConnected => client.Connected;

    private TcpClient client;
    private TCPClientInfo selfInfo;
    private NetworkStream serverStream;
    private StreamWriter writer;
    private StreamReader reader;
    private CancellationTokenSource cancelTokenSource = new();
    private Task listenerTask = null;
    private Dictionary<Guid, Response<Packet>> pendingResponses = [];

    /// <summary>
    /// Connects to a server with the given hostname and port.
    /// </summary>
    /// <param name="hostname"></param>
    /// <param name="port"></param>
    /// <returns>True when the connection has been established, otherwise false.</returns>
    public bool Connect(string hostname, int port, string username = "annonymous") => Connect(IPAddress.Parse(hostname), port, username);

    /// <summary>
    /// Connects to a server with the given hostname and port.
    /// </summary>
    /// <returns>True when the connection has been established, otherwise false.</returns>
    public bool Connect(IPAddress address, int port, string username = "annonymous")
    {
        if (client is not null && IsConnected)
            return true;

        try
        {
            client = new TcpClient();
            client.Connect(address, port);

            EnableKeepAlive(client, 60000, 10000);

            serverStream = client.GetStream();
            writer = new StreamWriter(serverStream);
            reader = new StreamReader(serverStream);

            listenerTask = Task.Run(() => Listener(cancelTokenSource.Token));

            string myGuid = SendAndResponse("^myGUID^").Payload;
            selfInfo = new TCPClientInfo(client);
            selfInfo.Id = Guid.Parse(myGuid);

            SendAndResponse($"^setusername^{username}");

            return true;
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Socket Exception: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Connects to a server on the same machine as the client, using the given port.
    /// </summary>
    /// <param name="port"></param>
    public bool ConnectSelf(int port)
    {
        return Connect(IPAddress.Loopback.ToString(), port);
    }

    public async Task<Packet> SendAndResponseAsync(string message, Guid? targetClientId = null)
    {
        var requestId = Guid.NewGuid();
        var tcs = new Response<Packet>();

        var payload = targetClientId.HasValue ? $"${targetClientId}:{requestId}:{message}" : $"${requestId}:{message}";

        Send(payload);

        // now wait for the response to come back. Response messages are in the format of "~&&~{requestId}:{message}"
        lock (pendingResponses)
        {
            pendingResponses.Add(requestId, tcs);
        }

        Packet res = await tcs;  // Await the response

        lock (pendingResponses)
        {
            pendingResponses.Remove(requestId);
        }

        return res;
    }

    public Packet SendAndResponse(string mesasge, Guid? targetClientId = null)
    {
        return SendAndResponseAsync(mesasge, targetClientId).GetAwaiter().GetResult();
    }

    public void SendResponse(string response, Guid requestId)
    {
        var payload = $"~&&~{requestId}:{response}";
        Send(payload, null);
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Send(string message, Guid? targetClientId = null)
    {
        if (IsDisposed) throw new ObjectDisposedException("TCPUser");

        var payload = targetClientId.HasValue ? $"{targetClientId}:{message}" : message;

        Packet p = new(payload, selfInfo);
        
        writer.WriteLine(p.GetSerialized());
        writer.Flush();
    }

    /// <summary>
    /// Requests the client to disconnect from the server.
    /// </summary>
    public bool Disconnect()
    {
        string a = SendAndResponse("^disconnect^").Payload;
        Dispose();
        return a is "OK";
    }

    /// <summary>
    /// Closes the connection to the server and disposes of all resources. The server will not be notified of the disconnect. Consider using <see cref="Disconnect"/> instead.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed) return;

        IsDisposed = true;
        cancelTokenSource.Cancel();
        reader?.Dispose();
        writer?.Dispose();
        serverStream?.Dispose();
        client?.Close();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asks the connected server for a list of all connected clients. This will include this client.
    /// </summary>
    /// <returns></returns>
    public string[] GetAllConnectionsFromServer()
    {
        return SendAndResponse("^connectionlist^").Payload.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>
    /// Asks the server for information about a specific client.
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    public string GetConnectionInfo(Guid clientId)
    {
        return SendAndResponse($"^getclientinfo^{clientId}").Payload;
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

    private async Task Listener(CancellationToken cancelToken)
    {
        try
        {
            string lastMessage = string.Empty;
            while (!cancelToken.IsCancellationRequested)
            {
                string? serializedPacket = await reader.ReadLineAsync();
                Packet packet = Packet.FromSerialized(serializedPacket);
                if (packet.Payload != null)
                {
                    _ = HandleMessage(packet);

                }
                else
                {
                    Console.WriteLine("Server closed the connection.");
                    OnServerClosed.Invoke();
                    Dispose();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client Listener Exception: {ex.Message}");
            Dispose();
        }
    }

    private async Task HandleMessage(Packet packet)
    {
        string message = packet.Payload;
        bool isResponse = message.StartsWith("~&&~");
        bool isRequest = message.StartsWith("$");

        message = message.TrimStart('$').TrimStart("~&&~".ToCharArray());

        var parts = message.Split(':', 3);
        if ((isRequest || isResponse) && Guid.TryParse(parts[0], out Guid requestId))
        {
            // there is an incomming request that requires a response
            if (isRequest)
            {
                if (parts[1] == "^ping^")
                {
                    SendResponse("pong", requestId);
                    return;
                }
                ResponseMessageReceived?.Invoke(parts[1], requestId);
                return;
            }

            // we got a response from a request we sent.
            var response = parts[1];
            Response<Packet>? tcs;
            lock (pendingResponses)
            {
                pendingResponses.TryGetValue(requestId, out tcs);
            }

            tcs?.SetResult(new Packet(response, packet.Sender, packet.FromServer));  // Set the result of the TaskCompletionSource
            return;
        }
        else
        {
            var payload = parts.Length >= 1 ? parts[0] : string.Empty;
            OnMessageReceived?.Invoke(payload, this, null);
        }
    }
}
