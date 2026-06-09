using WinterRose.ForgeVein.Networking.Defaults;
using WinterRose.ForgeVein.Networking.Diagnostics;
using WinterRose.ForgeVein.Networking.Packets;
using WinterRose.ForgeVein.Networking.Routing;
using WinterRose.ForgeVein.Networking.Session;
using WinterRose.ForgeVein.Networking.Validation;
using WinterRose.Recordium;

namespace WinterRose.ForgeVein.Networking.Application;

/// <summary>
/// Factory for quickly setting up a complete WinterRose.ForgeVein networking stack.
/// Provides sensible defaults while keeping all components replaceable.
/// </summary>
public sealed class NetworkApplicationFactory
{
    private readonly Log logger;
    private ISessionManager? sessionManager;
    private IPacketRegistry? packetRegistry;
    private IPacketHandlerRegistry? handlerRegistry;
    private IValidationRegistry? validationRegistry;
    private IValidationPipeline? validationPipeline;
    private IRoutingStrategy? routingStrategy;
    private IPacketDispatcher? dispatcher;
    private IDiagnosticsProvider? diagnostics;
    private IRelayService? relayService;
    private IStreamRouter? streamRouter;
    private IClusterDelegationService? delegationService;

    public NetworkApplicationFactory(Log? logger = null)
    {
        this.logger = logger ?? new Log("WinterRose.ForgeVein");
    }

    /// <summary>
    /// Creates a complete networking stack with default implementations.
    /// </summary>
    public NetworkApplicationStack CreateDefault()
    {
        sessionManager ??= new DefaultSessionManager();
        packetRegistry ??= new DefaultPacketRegistry();
        handlerRegistry ??= new DefaultPacketHandlerRegistry();
        validationRegistry ??= new DefaultValidationRegistry();
        validationPipeline ??= new DefaultValidationPipeline(validationRegistry);
        routingStrategy ??= new DefaultRoutingStrategy();
        dispatcher ??= new DefaultPacketDispatcher(packetRegistry, handlerRegistry, logger);
        diagnostics ??= new DefaultDiagnosticsProvider(sessionManager);
        relayService ??= new DefaultRelayService(sessionManager);
        streamRouter ??= new DefaultStreamRouter();
        delegationService ??= new DefaultClusterDelegationService();

        return new NetworkApplicationStack(
            sessionManager,
            packetRegistry,
            handlerRegistry,
            validationRegistry,
            validationPipeline,
            routingStrategy,
            dispatcher,
            diagnostics,
            relayService,
            streamRouter,
            delegationService,
            logger
        );
    }

    /// <summary>
    /// Allows custom session manager implementation.
    /// </summary>
    public NetworkApplicationFactory WithSessionManager(ISessionManager manager)
    {
        sessionManager = manager ?? throw new ArgumentNullException(nameof(manager));
        return this;
    }

    /// <summary>
    /// Allows custom packet registry implementation.
    /// </summary>
    public NetworkApplicationFactory WithPacketRegistry(IPacketRegistry registry)
    {
        packetRegistry = registry ?? throw new ArgumentNullException(nameof(registry));
        return this;
    }

    /// <summary>
    /// Allows custom handler registry implementation.
    /// </summary>
    public NetworkApplicationFactory WithHandlerRegistry(IPacketHandlerRegistry registry)
    {
        handlerRegistry = registry ?? throw new ArgumentNullException(nameof(registry));
        return this;
    }

    /// <summary>
    /// Allows custom validation registry implementation.
    /// </summary>
    public NetworkApplicationFactory WithValidationRegistry(IValidationRegistry registry)
    {
        validationRegistry = registry ?? throw new ArgumentNullException(nameof(registry));
        return this;
    }

    /// <summary>
    /// Allows custom validation pipeline implementation.
    /// </summary>
    public NetworkApplicationFactory WithValidationPipeline(IValidationPipeline pipeline)
    {
        validationPipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        return this;
    }

    /// <summary>
    /// Allows custom routing strategy implementation.
    /// </summary>
    public NetworkApplicationFactory WithRoutingStrategy(IRoutingStrategy strategy)
    {
        routingStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        return this;
    }

    /// <summary>
    /// Allows custom packet dispatcher implementation.
    /// </summary>
    public NetworkApplicationFactory WithDispatcher(IPacketDispatcher disp)
    {
        dispatcher = disp ?? throw new ArgumentNullException(nameof(disp));
        return this;
    }
}

/// <summary>
/// Complete networking application stack with all components.
/// </summary>
public sealed class NetworkApplicationStack
{
    public ISessionManager SessionManager { get; }
    public IPacketRegistry PacketRegistry { get; }
    public IPacketHandlerRegistry HandlerRegistry { get; }
    public IValidationRegistry ValidationRegistry { get; }
    public IValidationPipeline ValidationPipeline { get; }
    public IRoutingStrategy RoutingStrategy { get; }
    public IPacketDispatcher Dispatcher { get; }
    public IDiagnosticsProvider Diagnostics { get; }
    public IRelayService RelayService { get; }
    public IStreamRouter StreamRouter { get; }
    public IClusterDelegationService DelegationService { get; }
    public Log Logger { get; }

    public NetworkApplicationStack(
        ISessionManager sessionManager,
        IPacketRegistry packetRegistry,
        IPacketHandlerRegistry handlerRegistry,
        IValidationRegistry validationRegistry,
        IValidationPipeline validationPipeline,
        IRoutingStrategy routingStrategy,
        IPacketDispatcher dispatcher,
        IDiagnosticsProvider diagnostics,
        IRelayService relayService,
        IStreamRouter streamRouter,
        IClusterDelegationService delegationService,
        Log logger)
    {
        SessionManager = sessionManager;
        PacketRegistry = packetRegistry;
        HandlerRegistry = handlerRegistry;
        ValidationRegistry = validationRegistry;
        ValidationPipeline = validationPipeline;
        RoutingStrategy = routingStrategy;
        Dispatcher = dispatcher;
        Diagnostics = diagnostics;
        RelayService = relayService;
        StreamRouter = streamRouter;
        DelegationService = delegationService;
        Logger = logger;
    }
}
