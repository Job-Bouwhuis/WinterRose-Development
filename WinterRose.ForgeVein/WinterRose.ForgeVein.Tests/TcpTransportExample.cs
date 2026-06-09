using WinterRose.ForgeVein.Networking.Application;
using WinterRose.ForgeVein.Networking.Defaults;
using WinterRose.ForgeVein.Networking.Diagnostics;
using WinterRose.ForgeVein.Networking.Packets;
using WinterRose.ForgeVein.Networking.Routing;
using WinterRose.ForgeVein.Networking.Session;
using WinterRose.ForgeVein.Networking.Transport;
using WinterRose.ForgeVein.Networking.Validation;
using WinterRose.Recordium;
using System.Net;

namespace WinterRose.ForgeVein.Tests;

/// <summary>
/// Example demonstrating the WinterRose.ForgeVein architecture with TCP transport.
/// </summary>
public class TcpTransportExample
{
    public static async Task Main()
    {
        Console.WriteLine("=== WinterRose.ForgeVein TCP Transport Example ===\n");

        var logger = new Log("WinterRose.ForgeVein.Tests");

        // 1. Create packet registry
        var packetRegistry = new DefaultPacketRegistry();
        var serializer = new WinterForgePacketSerializer();

        // Register a simple packet (example: PingPacket)
        var pingPacketId = Guid.NewGuid();
        var pingMetadata = new PacketMetadata(
            Name: "Ping",
            Category: PacketCategory.Control,
            Reliability: ReliabilityMode.Reliable,
            RequiresValidation: false,
            RequiresRouting: false
        );
        var pingDescriptor = new DefaultPacketDescriptor(pingPacketId, typeof(string), pingMetadata, serializer);
        packetRegistry.Register(pingDescriptor);

        // 2. Create session manager
        var sessionManager = new DefaultSessionManager();

        // 3. Create packet handler registry
        var handlerRegistry = new DefaultPacketHandlerRegistry();

        // Register a ping handler
        handlerRegistry.Register<string>(new SimpleStringHandler());

        // 4. Create validation infrastructure
        var validationRegistry = new DefaultValidationRegistry();
        var validationPipeline = new DefaultValidationPipeline(validationRegistry);

        // 5. Create routing infrastructure
        var routingStrategy = new DefaultRoutingStrategy();

        // 6. Create dispatcher
        var dispatcher = new DefaultPacketDispatcher(packetRegistry, handlerRegistry, logger);

        // 7. Create diagnostics provider
        var diagnostics = new DefaultDiagnosticsProvider(sessionManager);

        // 8. Create TCP listener
        var endpoint = new IPEndPoint(IPAddress.Loopback, 9000);
        var tcpListener = new TcpTransportListener(endpoint);

        Console.WriteLine($"Starting TCP listener on {endpoint}...\n");

        await tcpListener.StartAsync();

        // 9. Accept one connection and demonstrate communication
        var acceptTask = tcpListener.AcceptAsync().AsTask();

        // Simulate client connecting after a short delay
        _ = Task.Delay(1000).ContinueWith(async _ =>
        {
            try
            {
                var clientEndpoint = new IPEndPoint(IPAddress.Loopback, 0);
                var clientSocket = new System.Net.Sockets.TcpClient();
                await clientSocket.ConnectAsync("127.0.0.1", 9000);
                Console.WriteLine("Client connected to server.\n");

                // Send a simple ping
                var networkStream = clientSocket.GetStream();
                var data = System.Text.Encoding.UTF8.GetBytes("Hello from client!");
                var lengthBytes = BitConverter.GetBytes(data.Length);
                Array.Reverse(lengthBytes);
                await networkStream.WriteAsync(lengthBytes, 0, 4);
                await networkStream.WriteAsync(data, 0, data.Length);
                await networkStream.FlushAsync();

                Console.WriteLine("Client sent message.\n");
                clientSocket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
        });

        try
        {
            // Wait for connection with timeout
            if (await Task.WhenAny(acceptTask, Task.Delay(5000)) == acceptTask)
            {
                var transportConnection = await acceptTask;

                // Create session
                var session = await sessionManager.CreateSessionAsync(transportConnection);
                Console.WriteLine($"Session created: {session.SessionId}\n");

                // Receive message
                try
                {
                    var data = await session.ReceiveAsync(CancellationToken.None);
                    var message = System.Text.Encoding.UTF8.GetString(data.Span);
                    Console.WriteLine($"Server received: {message}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving: {ex.Message}");
                }

                // Get diagnostics
                var sessionDiags = diagnostics.GetSessions().ToList();
                Console.WriteLine($"Active sessions: {sessionDiags.Count}");
                foreach (var diag in sessionDiags)
                {
                    Console.WriteLine($"  Session {diag.SessionId}:");
                    Console.WriteLine($"    Bytes sent: {diag.Statistics.BytesSent}");
                    Console.WriteLine($"    Bytes received: {diag.Statistics.BytesReceived}");
                    Console.WriteLine($"    Packets sent: {diag.Statistics.PacketsSent}");
                    Console.WriteLine($"    Packets received: {diag.Statistics.PacketsReceived}");
                }

                await session.CloseAsync();
            }
        }
        finally
        {
            await tcpListener.StopAsync();
            await tcpListener.DisposeAsync();
            await sessionManager.DisposeAsync();
        }

        Console.WriteLine("\nExample completed.");
    }
}

public class SimpleStringHandler : IPacketHandler<string>
{
    public ValueTask HandleAsync(ISessionConnection session, string packet, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Handler] Received string packet: {packet}");
        return ValueTask.CompletedTask;
    }
}
