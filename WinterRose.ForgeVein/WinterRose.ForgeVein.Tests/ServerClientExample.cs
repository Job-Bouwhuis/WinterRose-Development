using System.Net;
using WinterRose.ForgeVein.Networking.Application;
using WinterRose.ForgeVein.Networking.Defaults;
using WinterRose.ForgeVein.Networking.Packets;
using WinterRose.ForgeVein.Networking.Session;
using WinterRose.ForgeVein.Networking.Transport;
using WinterRose.Recordium;

namespace WinterRose.ForgeVein.Tests;

/// <summary>
/// Comprehensive example showing how to create custom server and client implementations.
/// </summary>
public class ServerClientExample
{
    public static void ShowArchitecturePattern()
    {
        Console.WriteLine("=== WinterRose.ForgeVein Server/Client Architecture ===\n");

        // 1. Create the application factory
        var logger = new Log("WinterRose.ForgeVein.Example");
        var factory = new NetworkApplicationFactory(logger);
        var stack = factory.CreateDefault();

        Console.WriteLine($"Created stack with:");
        Console.WriteLine($"  - Session Manager: {stack.SessionManager.GetType().Name}");
        Console.WriteLine($"  - Packet Registry: {stack.PacketRegistry.GetType().Name}");
        Console.WriteLine($"  - Validation Pipeline: {stack.ValidationPipeline.GetType().Name}");
        Console.WriteLine($"  - Routing Strategy: {stack.RoutingStrategy.GetType().Name}");
        Console.WriteLine($"  - Diagnostics: {stack.Diagnostics.GetType().Name}\n");

        // 2. Register a custom handler
        stack.HandlerRegistry.Register<TestPacket>(new TestPacketHandler());
        Console.WriteLine("Registered TestPacket handler\n");

        // 3. Setup packet registration
        var serializer = new WinterForgePacketSerializer();
        var packetId = Guid.NewGuid();
        var metadata = new PacketMetadata(
            Name: "TestPacket",
            Category: PacketCategory.Control,
            Reliability: ReliabilityMode.Reliable,
            RequiresValidation: false,
            RequiresRouting: false
        );
        var descriptor = new DefaultPacketDescriptor(packetId, typeof(TestPacket), metadata, serializer);
        stack.PacketRegistry.Register(descriptor);
        Console.WriteLine($"Registered TestPacket with ID: {packetId}\n");

        // 4. Show how to create transports
        var tcpListener = TransportFactory.CreateTcpListener("127.0.0.1", 9000);
        var udpListener = TransportFactory.CreateUdpListener("127.0.0.1", 9001);

        Console.WriteLine($"Created transports:");
        Console.WriteLine($"  - TCP on 127.0.0.1:9000");
        Console.WriteLine($"  - UDP on 127.0.0.1:9001\n");

        // 5. Show session statistics
        Console.WriteLine("Architecture is ready for:");
        Console.WriteLine("  ✓ Accepting connections");
        Console.WriteLine("  ✓ Managing sessions");
        Console.WriteLine("  ✓ Dispatching packets");
        Console.WriteLine("  ✓ Validating data");
        Console.WriteLine("  ✓ Routing packets");
        Console.WriteLine("  ✓ Monitoring diagnostics\n");
    }
}

public class TestPacket
{
    public string Message { get; set; } = string.Empty;
}

public class TestPacketHandler : IPacketHandler<TestPacket>
{
    public ValueTask HandleAsync(ISessionConnection session, TestPacket packet, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Handler] Received TestPacket: {packet.Message}");
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Example custom server implementation extending NetworkServerBase.
/// </summary>
public class ExampleNetworkServer : NetworkServerBase
{
    private ITransportListener? listener;
    private CancellationTokenSource? cancellationTokenSource;

    public ExampleNetworkServer(
        ISessionManager sessionManager,
        IPacketDispatcher dispatcher,
        IValidationPipeline validationPipeline,
        IRoutingStrategy routingStrategy,
        IPacketHandlerRegistry handlerRegistry)
        : base(sessionManager, dispatcher, validationPipeline, routingStrategy, handlerRegistry)
    {
    }

    public override async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = new IPEndPoint(IPAddress.Any, 9000);
        listener = TransportFactory.CreateTcpListener(endpoint);

        await listener.StartAsync(cancellationToken).ConfigureAwait(false);
        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Start accepting connections
        _ = AcceptConnectionsAsync(cancellationTokenSource.Token);
    }

    public override async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationTokenSource?.Cancel();
        if (listener != null)
        {
            await listener.StopAsync(cancellationToken).ConfigureAwait(false);
            await listener.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var transport = await listener!.AcceptAsync(cancellationToken).ConfigureAwait(false);
                var session = await SessionManager.CreateSessionAsync(transport, cancellationToken).ConfigureAwait(false);

                // Handle session in background
                _ = HandleSessionAsync(session, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Server shutting down
        }
    }

    private async Task HandleSessionAsync(ISessionConnection session, CancellationToken cancellationToken)
    {
        try
        {
            while (session.State == SessionState.Active && !cancellationToken.IsCancellationRequested)
            {
                var data = await session.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                if (data.IsEmpty)
                    break;

                await Dispatcher.DispatchAsync(session, data, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            // Log error
        }
        finally
        {
            await session.CloseAsync().ConfigureAwait(false);
        }
    }
}

/// <summary>
/// Example custom client implementation extending NetworkClientBase.
/// </summary>
public class ExampleNetworkClient : NetworkClientBase
{
    private ISessionConnection? session;
    private readonly string address;
    private readonly int port;

    public ExampleNetworkClient(
        ISessionManager sessionManager,
        IPacketDispatcher dispatcher,
        string address,
        int port)
        : base(sessionManager, dispatcher)
    {
        this.address = address;
        this.port = port;
    }

    public override async ValueTask ConnectAsync(CancellationToken cancellationToken = default)
    {
        // This is a simplified example
        // In reality, you'd create a TCP socket and connect
        throw new NotImplementedException("Client connection logic would be implemented here");
    }

    public override async ValueTask DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (session != null)
        {
            await session.CloseAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        if (session == null)
            throw new InvalidOperationException("Not connected");

        await session.SendAsync(data, ReliabilityMode.Reliable, cancellationToken).ConfigureAwait(false);
    }
}
