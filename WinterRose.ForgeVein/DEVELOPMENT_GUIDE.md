# WinterRose.ForgeVein - Development Guide

## Architecture Overview

WinterRose.ForgeVein uses a **layered, interface-first architecture** with clear separation of concerns. Understanding this structure is crucial for contributing effectively.

### Layer Dependencies

```
Application Layer (Server/Client)
    ↓
Routing & Relay Layer
    ↓
Validation Layer
    ↓
Packet & Serialization Layer
    ↓
Session Layer
    ↓
Transport Layer (TCP/UDP)
```

## Key Design Principles

### 1. Interface-First Design
Every layer defines interfaces before implementations. This enables:
- Easy testing with mocks
- Swappable implementations
- Clear contracts

```csharp
// Good: Start with interface
public interface IPacketRegistry
{
    void Register(IPacketDescriptor descriptor);
    bool TryGetDescriptor(Guid packetId, out IPacketDescriptor descriptor);
}

// Then provide default implementation
public sealed class DefaultPacketRegistry : IPacketRegistry
{
    // Implementation
}
```

### 2. No Layering Violations
- Transport never knows about sessions
- Session never knows about packets
- Packet never knows about validation
- Validation never knows about routing
- Routing never knows about application logic

Each layer depends only on layers below it.

### 3. Explicit Over Implicit
- No magic, all behavior is explicit
- No attribute-based configuration
- No automatic discovery
- Developers control every step

### 4. Thread Safety
All implementations must be thread-safe where needed:
- Use `ConcurrentDictionary` for registries
- Use locks for shared mutable state
- Mark non-thread-safe classes clearly

### 5. Async/Await Throughout
All I/O operations use `async`/`await`:
- Use `ValueTask` for frequently-called methods
- Use `Task` for background operations
- Properly propagate `CancellationToken`

## Adding New Features

### Adding a New Packet Type

1. Define the packet class (no special base class needed):
```csharp
public class LoginPacket
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

2. Create a handler:
```csharp
public class LoginPacketHandler : IPacketHandler<LoginPacket>
{
    public ValueTask HandleAsync(ISessionConnection session, LoginPacket packet, CancellationToken cancellationToken = default)
    {
        // Handle login
        return ValueTask.CompletedTask;
    }
}
```

3. Register the packet and handler:
```csharp
var packetId = Guid.NewGuid();
var metadata = new PacketMetadata(
    Name: "Login",
    Category: PacketCategory.Authentication,
    Reliability: ReliabilityMode.Reliable,
    RequiresValidation: true,
    RequiresRouting: false
);
var descriptor = new DefaultPacketDescriptor(
    packetId, typeof(LoginPacket), metadata, serializer);

registry.Register(descriptor);
handlerRegistry.Register<LoginPacket>(new LoginPacketHandler());
```

### Adding Validation

1. Define a validation definition:
```csharp
public class LoginValidator : IValidationDefinition<LoginPacket>
{
    public void Define(IValidationBuilder<LoginPacket> builder)
    {
        builder.RuleFor(x => x.Username)
            .MinLength(3)
            .WithMessage("Username must be at least 3 characters.");

        builder.RuleFor(x => x.Password)
            .MinLength(8)
            .WithMessage("Password must be at least 8 characters.");
    }

    public IValidationResult Validate(LoginPacket instance)
    {
        var validationBuilder = new ValidationBuilder<LoginPacket>();
        Define(validationBuilder);
        return validationBuilder.Validate(instance);
    }
}
```

2. Register the validator:
```csharp
validationRegistry.Register<LoginPacket>(new LoginValidator());
```

3. Mark packet as requiring validation:
```csharp
var metadata = new PacketMetadata(
    // ... other properties
    RequiresValidation: true
);
```

### Implementing Custom Routing

1. Create a custom routing strategy:
```csharp
public class CustomRoutingStrategy : IRoutingStrategy
{
    public IRoutingDecision Decide(ISessionConnection session, IPacketDescriptor descriptor, ReadOnlyMemory<byte> payload)
    {
        // Custom logic to determine route
        if (descriptor.Metadata.Category == PacketCategory.Relay)
        {
            return new DefaultRoutingDecision(
                new RouteDescriptor(
                    Kind: RouteKind.Relay,
                    DestinationSession: someSessionId,
                    StreamId: null,
                    ClusterService: null
                )
            );
        }

        return new DefaultRoutingDecision(
            new RouteDescriptor(RouteKind.Direct, null, null, null)
        );
    }
}
```

2. Use in your application:
```csharp
var customStrategy = new CustomRoutingStrategy();
var dispatcher = new DefaultPacketDispatcher(registry, handlerRegistry, logger);
```

## Testing Guidelines

### Unit Testing
- Mock all dependencies using the interface contracts
- Test each layer independently
- Don't create real TCP/UDP sockets in unit tests

```csharp
[Test]
public void PacketRegistry_RegistersPacketDescriptor()
{
    var registry = new DefaultPacketRegistry();
    var descriptor = new DefaultPacketDescriptor(
        Guid.NewGuid(), typeof(string), 
        new PacketMetadata(...), 
        new MockSerializer()
    );

    registry.Register(descriptor);

    Assert.That(
        registry.TryGetDescriptor(descriptor.PacketId, out var found),
        Is.True
    );
    Assert.That(found, Is.EqualTo(descriptor));
}
```

### Integration Testing
- Use real transports in isolated test
- Test end-to-end packet flow
- Clean up resources properly

```csharp
[Test]
public async Task Server_ReceivesPacketFromClient()
{
    var listener = new TcpTransportListener(new IPEndPoint(IPAddress.Loopback, 0));
    await listener.StartAsync();

    // Connect client and verify communication

    await listener.StopAsync();
    await listener.DisposeAsync();
}
```

## Performance Considerations

### Hotpath Optimization
- Hotpath packets bypass validation
- Use `ReliabilityMode.HotPath` for low-latency packets
- Avoid reflection in packet dispatch (future optimization)

### Memory Efficiency
- Use `ReadOnlyMemory<byte>` to avoid copies
- Return pooled objects where possible
- Profile before optimizing

### Concurrency
- Use concurrent collections for thread-safe registries
- Minimize lock contention
- Avoid blocking operations in async code

## Code Standards

### Naming
- Interfaces: `I` prefix (e.g., `IPacketRegistry`)
- Classes: PascalCase (e.g., `DefaultPacketRegistry`)
- Private fields: camelCase (e.g., `_buffer`)
- Constants: UPPER_SNAKE_CASE (e.g., `MAX_PACKET_SIZE`)

### Documentation
- Public APIs must have XML documentation
- Document thread safety guarantees
- Explain non-obvious design decisions

```csharp
/// <summary>
/// Registers a packet descriptor in the registry.
/// This method is thread-safe.
/// </summary>
/// <param name="descriptor">The packet descriptor to register</param>
/// <exception cref="ArgumentNullException">Thrown when descriptor is null</exception>
/// <exception cref="InvalidOperationException">Thrown when packet ID already registered</exception>
public void Register(IPacketDescriptor descriptor)
{
    // Implementation
}
```

### Error Handling
- Use descriptive exception messages
- Prefer checked exceptions for known failures
- Use `ArgumentNullException` for null checks
- Wrap transport errors in higher-level exceptions

```csharp
if (descriptor == null)
    throw new ArgumentNullException(nameof(descriptor));

try
{
    // Transport operation
}
catch (IOException ex)
{
    throw new InvalidOperationException("Failed to send packet", ex);
}
```

## Debugging Tips

### Trace Packet Flow
1. Enable logging at all layers
2. Use unique IDs for tracking (SessionId, PacketId)
3. Log at entry/exit of handlers

### Memory Leaks
- Ensure all `IAsyncDisposable` are properly disposed
- Use using statements for resource management
- Run memory profiler on long-running tests

### Concurrency Issues
- Use thread-safe testing utilities
- Test with high concurrency (100+ threads)
- Look for deadlocks in lock usage

## Future Optimization Opportunities

### 1. Reflection-Free Dispatch
Currently uses reflection for handler invocation. Future optimization:
- Source generate dispatch methods
- Create delegates at registration time
- Maintain zero-reflection hotpath

### 2. Memory Pooling
- Pool buffers for packet data
- Pool session objects
- Pool validation builders

### 3. JIT Optimization
- Consider ReadOnly ref structs
- Optimize hot methods
- Profile with benchmarks

### 4. Encryption Pipeline
- Implement `IEncryptionPipeline`
- Add RSA and hybrid encryption
- Implement key exchange

## Contributing Checklist

- [ ] Interface defined before implementation
- [ ] Thread safety documented and implemented
- [ ] All public methods have XML docs
- [ ] Error handling is proper and informative
- [ ] No layer violations
- [ ] Async/await used throughout
- [ ] Proper resource cleanup (IAsyncDisposable)
- [ ] Unit tests pass
- [ ] Integration tests added
- [ ] Code reviewed for maintainability

## Resources

- WinterRose.WinterForge - Serialization library
- WinterRose.ForgeSignal - Event bus (future integration)
- WinterRose.ForgeThread - Threading utilities
- WinterRose.Recordium - Logging
