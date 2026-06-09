# WinterRose.ForgeVein - Quick Start Guide

Get up and running with WinterRose.ForgeVein in 5 minutes!

## Installation

The WinterRose.ForgeVein project is already set up. Just ensure you have:
- .NET 10 SDK
- Visual Studio 2026 (Community or higher)

## Basic Setup

### 1. Create the Application Stack

```csharp
using WinterRose.ForgeVein.Networking.Application;
using WinterRose.Recordium;

// Create logger
var logger = new Log("MyApp");

// Create full application stack with defaults
var factory = new NetworkApplicationFactory(logger);
var stack = factory.CreateDefault();
```

Now you have access to:
- `stack.SessionManager` - Manage connections
- `stack.PacketRegistry` - Register packet types
- `stack.HandlerRegistry` - Register handlers
- `stack.ValidationPipeline` - Validate packets
- `stack.RoutingStrategy` - Route packets
- `stack.Diagnostics` - Monitor sessions

### 2. Define Your First Packet

```csharp
public class GreetingPacket
{
    public string Message { get; set; } = string.Empty;
}

public class GreetingHandler : IPacketHandler<GreetingPacket>
{
    public ValueTask HandleAsync(
        ISessionConnection session, 
        GreetingPacket packet, 
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Received: {packet.Message}");
        return ValueTask.CompletedTask;
    }
}
```

### 3. Register the Packet

```csharp
using WinterRose.ForgeVein.Networking.Packets;
using WinterRose.ForgeVein.Networking.Defaults;
using WinterRose.ForgeVein.Networking.Transport;

var serializer = new WinterForgePacketSerializer();
var packetId = Guid.NewGuid();

var metadata = new PacketMetadata(
    Name: "Greeting",
    Category: PacketCategory.Control,
    Reliability: ReliabilityMode.Reliable,
    RequiresValidation: false,
    RequiresRouting: false
);

var descriptor = new DefaultPacketDescriptor(
    packetId, 
    typeof(GreetingPacket), 
    metadata, 
    serializer
);

stack.PacketRegistry.Register(descriptor);
stack.HandlerRegistry.Register<GreetingPacket>(new GreetingHandler());
```

### 4. Create a TCP Server

```csharp
using System.Net;

var endpoint = new IPEndPoint(IPAddress.Any, 9000);
var listener = TransportFactory.CreateTcpListener(endpoint);

await listener.StartAsync();
Console.WriteLine("Server listening on port 9000...");

// Accept connections
while (true)
{
    var transport = await listener.AcceptAsync();
    var session = await stack.SessionManager.CreateSessionAsync(transport);

    // Handle session (in background or with Task)
    _ = HandleSessionAsync(session);
}
```

### 5. Process Packets

```csharp
async Task HandleSessionAsync(ISessionConnection session)
{
    try
    {
        while (session.State == SessionState.Active)
        {
            var data = await session.ReceiveAsync();
            if (data.IsEmpty) break;

            // Dispatch to handler
            await stack.Dispatcher.DispatchAsync(session, data);
        }
    }
    finally
    {
        await session.CloseAsync();
    }
}
```

## Common Tasks

### Add Validation

```csharp
public class GreetingValidator : IValidationDefinition<GreetingPacket>
{
    public void Define(IValidationBuilder<GreetingPacket> builder)
    {
        builder.RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message cannot be empty.")
            .MinLength(5)
            .WithMessage("Message must be at least 5 characters.");
    }

    public IValidationResult Validate(GreetingPacket instance)
    {
        var validationBuilder = new ValidationBuilder<GreetingPacket>();
        Define(validationBuilder);
        return validationBuilder.Validate(instance);
    }
}

// Register
stack.ValidationRegistry.Register<GreetingPacket>(
    new GreetingValidator());

// Update metadata
var metadata = new PacketMetadata(
    Name: "Greeting",
    Category: PacketCategory.Control,
    Reliability: ReliabilityMode.Reliable,
    RequiresValidation: true,  // ← Enable validation
    RequiresRouting: false
);
```

### Handle Multiple Packet Types

```csharp
// Handler 1
public class LoginHandler : IPacketHandler<LoginPacket>
{
    public ValueTask HandleAsync(ISessionConnection session, LoginPacket packet, CancellationToken ct = default)
    {
        // Handle login
        return ValueTask.CompletedTask;
    }
}

// Handler 2
public class DataPacketHandler : IPacketHandler<DataPacket>
{
    public ValueTask HandleAsync(ISessionConnection session, DataPacket packet, CancellationToken ct = default)
    {
        // Handle data
        return ValueTask.CompletedTask;
    }
}

// Register both
stack.HandlerRegistry.Register<LoginPacket>(new LoginHandler());
stack.HandlerRegistry.Register<DataPacket>(new DataPacketHandler());

// Register packets (repeat for each type)
// ...
```

### Monitor Sessions

```csharp
// Get all active sessions
var diagnostics = stack.Diagnostics.GetSessions();

foreach (var sessionDiag in diagnostics)
{
    Console.WriteLine($"Session: {sessionDiag.SessionId}");
    Console.WriteLine($"  Connected: {sessionDiag.Metadata.ConnectedAt}");
    Console.WriteLine($"  Bytes Sent: {sessionDiag.Statistics.BytesSent}");
    Console.WriteLine($"  Bytes Received: {sessionDiag.Statistics.BytesReceived}");
    Console.WriteLine($"  Packets Sent: {sessionDiag.Statistics.PacketsSent}");
    Console.WriteLine($"  Packets Received: {sessionDiag.Statistics.PacketsReceived}");
}
```

### Create UDP Server

```csharp
var endpoint = new IPEndPoint(IPAddress.Any, 9001);
var listener = TransportFactory.CreateUdpListener(endpoint);

await listener.StartAsync();
Console.WriteLine("UDP server listening on port 9001...");

// UDP same as TCP - accept connections
while (true)
{
    var transport = await listener.AcceptAsync();
    var session = await stack.SessionManager.CreateSessionAsync(transport);
    _ = HandleSessionAsync(session);
}
```

## Project Structure

- `WinterRose.ForgeVein` - Main library
  - `Transport/` - TCP/UDP implementations
  - `Session/` - Session management
  - `Packets/` - Packet abstractions
  - `Validation/` - Validation pipeline
  - `Routing/` - Routing and relay
  - `Application/` - Server/client bases

- `WinterRose.ForgeVein.Tests` - Examples and demos
  - `TcpTransportExample.cs` - TCP demo
  - `ValidationExample.cs` - Validation demo
  - `ServerClientExample.cs` - Architecture patterns

## Key Classes & Interfaces

| Type | Purpose |
|------|---------|
| `ITransportConnection` | Raw TCP/UDP connection |
| `ISessionConnection` | Logical session over transport |
| `IPacketDescriptor` | Packet metadata & serializer |
| `IPacketHandler<T>` | Handles specific packet type |
| `IValidator<T>` | Validates packet data |
| `IRoutingStrategy` | Routes packets to handlers |
| `IDiagnosticsProvider` | Session monitoring |
| `NetworkApplicationFactory` | Stack builder |

## Useful Enums

```csharp
// Transport protocol
enum TransportProtocol { Tcp, Udp }

// Session state
enum SessionState { Handshaking, Authenticated, Active, Closing, Closed }

// Packet reliability
enum ReliabilityMode { Reliable, Unreliable, HotPath }

// Packet categories
enum PacketCategory 
{ 
    Handshake, Control, Reliable, HotPath, Relay, 
    Stream, Diagnostics, Authentication 
}

// Routing type
enum RouteKind { Direct, Relay, Stream, Delegation }
```

## Troubleshooting

### "Port already in use"
```csharp
// Use a different port
var endpoint = new IPEndPoint(IPAddress.Any, 9002);
```

### "Handler not found" warning
Make sure handler is registered:
```csharp
// Register handler before receiving packets
stack.HandlerRegistry.Register<MyPacket>(new MyPacketHandler());
```

### "Packet deserialize failed"
Check that:
1. Packet class is registered with correct type
2. Serializer matches packet structure
3. Payload format is correct

## Performance Tips

1. **Use HotPath for high-frequency packets**
   ```csharp
   var metadata = new PacketMetadata(
       // ...
       Reliability: ReliabilityMode.HotPath  // Skips validation
   );
   ```

2. **Register all packets upfront**
   ```csharp
   // Register all at startup
   // Not dynamically during operation
   ```

3. **Use UDP for unreliable data**
   ```csharp
   Reliability: ReliabilityMode.Unreliable  // Faster
   ```

## Next Steps

1. Read `README.md` for architecture overview
2. Read `DEVELOPMENT_GUIDE.md` for contributing
3. Check examples in `Tests` folder
4. Start building your custom handlers and validators

## Examples Location

- TCP Server: `WinterRose.ForgeVein.Tests/TcpTransportExample.cs`
- Validation: `WinterRose.ForgeVein.Tests/ValidationExample.cs`
- Architecture: `WinterRose.ForgeVein.Tests/ServerClientExample.cs`

## Need Help?

- Check the comprehensive README.md
- Review DEVELOPMENT_GUIDE.md for patterns
- Look at example files for reference implementations
- All public APIs have XML documentation

Happy coding! 🚀
