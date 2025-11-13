using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.NetworkServer;
using WinterRose.NetworkServer.Connections;
using WinterRose.NetworkServer.Packets;
using WinterRose.Recordium;
using WinterRose.WinterForgeSerializing;

//
// Simple terminal chat using the existing WinterRose.NetworkServer package as server base.
// Usage:
//   dotnet run -- server [port]
//   dotnet run -- client <server-ip> <port> <username>
//

const int DefaultPort = 53802;
const string DefaultServerHost = "127.0.0.1";

LogDestinations.AddDestination(new ConsoleLogDestination());
LogDestinations.AddDestination(new FileLogDestination("networkserver_testapp"));

while (true)
{
    Console.WriteLine();
    Console.WriteLine("Choose mode: 'server' or 'client [username]'. Type 'quit' to exit.");
    Console.Write("> ");
    var line = Console.ReadLine();
    if (line == null) // EOF
        break;

    var trimmed = line.Trim();
    if (string.IsNullOrEmpty(trimmed))
        continue;

    if (trimmed.Equals("quit", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    // split first token as command, remainder join as username (allows spaces in username)
    var parts = trimmed.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
    var cmd = parts[0].ToLowerInvariant();
    string? inlineUsername = parts.Length > 1 ? parts[1].Trim() : null;

    if (cmd == "server")
    {
        await RunServerAsync(DefaultPort);
        // when server stops, return to prompt
    }
    else if (cmd == "client")
    {
        string username;
        if (!string.IsNullOrEmpty(inlineUsername))
        {
            username = inlineUsername;
        }
        else
        {
            Console.Write("Enter username: ");
            var u = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(u))
            {
                Console.WriteLine("Username cannot be empty. Aborting client startup.");
                continue;
            }
            username = u.Trim();
        }

        await RunClientAsync(DefaultServerHost, DefaultPort, username);
        // when client disconnects, return to prompt
    }
    else
    {
        Console.WriteLine("Unknown command. Use 'server' or 'client [username]'.");
    }
}


static async Task RunServerAsync(int port)
{
    Console.WriteLine($"Starting server on port {port}...");
    var server = new ServerConnection(IPAddress.Any, port, clusterID: null);
    server.Username = "Server";
    server.OnClientConnected += cc =>
    {
        Console.WriteLine($"Client connected: {cc.ClientEndpoint?.ToString() ?? "Unknown"}, username = {cc.Username}");
    };

    // Start server
    server.Start();
    Console.WriteLine($"Server running on port {server.Port}. Press Ctrl+C to stop.");

    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
    };

    // keep server alive until canceled
    try
    {
        while (!cts.Token.IsCancellationRequested)
            await Task.Delay(500, cts.Token);
    }
    catch (OperationCanceledException) { }

    Console.WriteLine("Stopping server...");
    server.Disconnect();
    Console.WriteLine("Server stopped.");
}

static async Task RunClientAsync(string host, int port, string username)
{
    Console.WriteLine($"Connecting to {host}:{port} as {username}...");
    var ip = IPAddress.Parse(host);
    var client = ClientConnection.Connect(ip, port, username);
    Console.WriteLine("Connected. Type messages and press Enter. Type /exit to quit.");

    // Try to set username on server (best-effort)
    try
    {
        client.SetUsername(username);
    }
    catch { /* ignore */ }

    // Input loop sending chat messages
    while (true)
    {
        Console.Write("> ");
        string? line = Console.ReadLine();
        if (line == null) break;
        if (line.Trim().Equals("/exit", StringComparison.OrdinalIgnoreCase)) break;
        // Build chat packet
        Packet p = new Packet()
        {
            Header = new BasicHeader("ChatMessage"),
            Content = new ChatStringContent(line)
        };
        try
        {
            // Send without waiting for response (fire-and-forget)
            client.Send(p);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Send failed: " + ex.Message);
            break;
        }
    }

    try { client.Disconnect(); } catch { }
    Console.WriteLine("Client closed.");
}

//
// Local helper packet content and handler types. Placed in this file so the test app
// and server share the same types (serialization name match).
//

namespace WinterRose.NetworkServer.Packets
{
    // Simple string content for chat messages
    public class ChatStringContent : PacketContent
    {
        public string Message { get; set; } = string.Empty;
        private ChatStringContent() { } // for deserialization
        public ChatStringContent(string message) { Message = message; }
    }

    // Packet handler for "ChatMessage" packets. This will be discovered automatically by the framework.
    public class MessagePacketHandler : PacketHandler
    {
        public override string Type => "ChatMessage";

        public override void Handle(Packet packet, NetworkConnection self, NetworkConnection sender)
        {
            // Extract message string using reflection (robust to content type)
            string messageText = ExtractStringFromContent(packet.Content);

            // If running on server: broadcast message to all clients (including sender)
            if (self is ServerConnection)
            {
                // Tag sender info is already on packet.SenderUsername / SenderID by the framework
                // Re-broadcast the same packet to all connected clients
                try
                {
                    self.Send(packet, false);
                }
                catch (Exception ex)
                {
                    logger?.Error(ex, "Failed to broadcast chat packet.");
                }
                return;
            }

            // If running on client: print message unless it originated from ourselves
            if (self is ClientConnection client)
            {
                // If sender is this client, skip (avoid echo) - SenderID may be zero until handshake completes
                if (packet.SenderID != Guid.Empty && packet.SenderID == client.Identifier)
                    return;

                string from = !string.IsNullOrEmpty(packet.SenderUsername) ? packet.SenderUsername : packet.SenderID.ToString();
                Console.WriteLine($"[{from}] {messageText}");
                return;
            }

            // fallback: just log
            logger?.Info("Message: " + messageText);
        }

        public override void HandleResponsePacket(ReplyPacket replyPacket, Packet packet, NetworkConnection self, NetworkConnection sender)
        {
            // Not used for chat messages in this simple app.
        }

        private static string ExtractStringFromContent(PacketContent content)
        {
            if (content == null)
                return string.Empty;

            var type = content.GetType();

            // Try common property names
            var prop = type.GetProperty("ChatMessage") ?? type.GetProperty("Value") ?? type.GetProperty("Text") ?? type.GetProperties().FirstOrDefault(p => p.PropertyType == typeof(string));
            if (prop != null)
            {
                var val = prop.GetValue(content);
                return val?.ToString() ?? string.Empty;
            }

            // Fallback: ToString()
            return content.ToString() ?? string.Empty;
        }
    }
}