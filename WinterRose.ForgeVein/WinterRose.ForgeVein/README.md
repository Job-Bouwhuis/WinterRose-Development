# WinterRose.ForgeVein - Networking Framework

A modular, extensible networking framework for .NET 10, designed as a learning and experimentation platform for distributed communication systems. The framework emphasizes maintainability, modularity, and strict separation of concerns.

## Architecture Overview

WinterRose.ForgeVein follows a layered, interface-first architecture with clear responsibility boundaries:

### Core Layers

1. **Transport Layer** - Raw byte communication and connection lifecycle management
   - `ITransportConnection` - Individual transport connection abstraction
   - `ITransportListener` - Transport server abstraction
   - Implementations: TCP and UDP

2. **Session Layer** - Logical connection management with reliability handling
   - `ISessionConnection` - Logical session abstraction
   - `ISessionManager` - Session lifecycle management
   - `IReliabilityHandler` - Packet reliability management
   - Default implementations with TCP/UDP-specific behavior

3. **Packet Layer** - Packet metadata, serialization, and registry
   - `IPacketDescriptor` - Packet metadata and serialization mapping
   - `IPacketRegistry` - Packet type registry
   - `IPacketSerializer` - Packet serialization abstraction
   - Support for packet categories and metadata

4. **Validation Layer** - Optional validation with fluent API
   - `IValidator<T>` - Packet validation
   - `IValidationDefinition<T>` - Fluent validation definitions
   - Hotpath packets skip validation
   - `IValidationRegistry` - Validator registration

5. **Routing Layer** - Packet routing and relay systems
   - `IRoutingStrategy` - Routing decision making
   - `IRelayService` - Inter-session relaying
   - `IStreamRouter` - Logical stream management
   - `IClusterDelegationService` - Cluster-aware delegation

6. **Encryption Layer** (Phase 6) - Pluggable encryption pipeline
   - `IEncryptionPipeline` - Encryption/decryption abstraction
   - `IEncryptionStateTracker` - Per-session encryption state
   - `IKeyExchangeProtocol` - Key negotiation

7. **Diagnostics Layer** - Optional diagnostics and monitoring
   - `IDiagnosticsProvider` - Session and packet statistics
   - Per-session metrics and metadata

## Key Features

### Transport Agnostic
- Single abstraction for both TCP and UDP
- Transport-specific reliability handling is transparent to developers
- TCP uses native reliability; UDP implements manual acknowledgment (future)

### Hotpath Optimization
- Hotpath packets bypass validation and minimize metadata
- Designed for low-latency, high-frequency packets
- Special `ReliabilityMode.HotPath` setting

### Packet Categories
- **Handshake** - Connection initialization
- **Control** - Control messages
- **Reliable** - Guaranteed delivery packets
- **HotPath** - High-frequency, low-latency packets
- **Relay** - Relayed packets
- **Stream** - Stream-based communication
- **Diagnostics** - Diagnostic information
- **Authentication** - Auth/identity packets

### Fluent Validation API
```csharp
var builder = new ValidationBuilder<MyPacket>();
builder.RuleFor(x => x.Name)
    .MinLength(5)
    .WithMessage("Name must be at least 5 characters.");
```

### Reflection-Free Dispatch (Future)
- Current implementation uses reflection for demonstration
- Planned optimization through delegate compilation and source generation
- No runtime reflection in hotpath

## Project Structure

```
WinterRose.ForgeVein/
├── Transport/
│   ├── TransportAbstractions.cs      # Core interfaces
│   ├── TcpTransportConnection.cs     # TCP implementation
│   ├── TcpTransportListener.cs       # TCP server
│   ├── UdpTransportConnection.cs     # UDP implementation
│   └── UdpTransportListener.cs       # UDP server
├── Session/
│   ├── SessionAbstractions.cs        # Core interfaces
│   ├── DefaultSessionConnection.cs   # Session implementation
│   └── DefaultSessionManager.cs      # Session manager
├── Packets/
│   ├── PacketAbstractions.cs         # Core interfaces
│   └── (Packet handlers and types)
├── Validation/
│   ├── ValidationAbstractions.cs     # Core interfaces
│   └── (Validation implementations)
├── Routing/
│   ├── RoutingAbstractions.cs        # Core interfaces
│   ├── DefaultRoutingStrategy.cs     # Default routing
│   ├── DefaultRelayService.cs        # Relay implementation
│   ├── DefaultStreamRouter.cs        # Stream management
│   └── DefaultClusterDelegationService.cs
├── Encryption/
│   └── EncryptionAbstractions.cs     # Encryption abstractions
├── Diagnostics/
│   ├── DiagnosticsAbstractions.cs    # Core interfaces
│   └── DefaultDiagnosticsProvider.cs
├── Application/
│   ├── ApplicationAbstractions.cs    # Server/Client bases
│   └── DefaultPacketHandlerRegistry.cs
└── Defaults/
    ├── DefaultPacketRegistry.cs
    ├── DefaultPacketDispatcher.cs
    ├── DefaultValidation.cs
    ├── DefaultValidationPipeline.cs
    └── WinterForgePacketSerializer.cs
```

## Usage Examples

### Basic TCP Server
```csharp
// Create infrastructure
var sessionManager = new DefaultSessionManager();
var packetRegistry = new DefaultPacketRegistry();
var handlerRegistry = new DefaultPacketHandlerRegistry();

// Register handler
handlerRegistry.Register<MyPacket>(new MyPacketHandler());

// Create listener
var endpoint = new IPEndPoint(IPAddress.Any, 9000);
var listener = new TcpTransportListener(endpoint);

await listener.StartAsync();

// Accept connection
var transport = await listener.AcceptAsync();
var session = await sessionManager.CreateSessionAsync(transport);
```

### Packet Registration
```csharp
var registry = new DefaultPacketRegistry();
var serializer = new WinterForgePacketSerializer();

var metadata = new PacketMetadata(
    Name: "UserLogin",
    Category: PacketCategory.Authentication,
    Reliability: ReliabilityMode.Reliable,
    RequiresValidation: true,
    RequiresRouting: false
);

var descriptor = new DefaultPacketDescriptor(
    packetId: Guid.NewGuid(),
    packetType: typeof(LoginPacket),
    metadata: metadata,
    serializer: serializer
);

registry.Register(descriptor);
```

### Validation
```csharp
// Define validation
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
        // Implementation
    }
}

// Register and use
var validationRegistry = new DefaultValidationRegistry();
var pipeline = new DefaultValidationPipeline(validationRegistry);
validationRegistry.Register(new LoginValidator());

var result = pipeline.ValidatePacket(loginPacket, metadata);
```

## Development Roadmap

### Phase 1 ✅ - Core Architecture (COMPLETED)
- Foundational project structure
- Transport abstractions (TCP/UDP)
- Packet abstractions with registry
- Base application classes

### Phase 2 - Session Layer & Reliability
- Session layer implementation
- Server-authoritative handshake
- UDP acknowledgment handling
- Session events (via ForgeSignal)

### Phase 3 - Validation & Processing
- Validation pipeline
- Fluent validator API
- Packet processing pipeline
- Hotpath optimization

### Phase 4 - Routing & Relay
- Routing abstractions
- Relay systems
- Substream support
- Stream lifecycle management

### Phase 5 - Cluster Delegation
- Cluster abstractions
- Authority server patterns
- Service discovery
- Delegation flow

### Phase 6 - Encryption & Diagnostics
- Pluggable encryption pipeline
- Key exchange abstractions
- Expanded diagnostics system
- UI-ready diagnostic reporting

## Dependencies

- WinterRose.WinterForge - Serialization
- WinterRose.ForgeSignal - Event bus
- WinterRose.ForgeThread - Threading utilities
- WinterRose.Recordium - Logging
- ForgeMantle - Utilities
- Microsoft.Extensions.Logging.Abstractions

## Design Principles

1. **Interface-First** - All abstractions are interface-based and replaceable
2. **Layered Architecture** - Clear separation between transport, session, packets, validation, and routing
3. **Transport Agnostic** - Same code works with TCP or UDP
4. **Reflection-Free Hotpath** - Zero reflection in critical paths
5. **Extensibility** - Developers inherit from base classes and implement interfaces
6. **No Magic** - Explicit, clear contracts without implicit behavior

## Contributing

This is a learning and experimentation framework. Focus areas:
- Clean architecture boundaries
- Strong abstraction design
- Transport system flexibility
- Distributed communication patterns

## License

Part of the WinterRose project ecosystem.
