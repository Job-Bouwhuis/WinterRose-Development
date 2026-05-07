using WinterRose.ForgeVein.Networking.Packets;
using WinterRose.ForgeVein.Networking.Session;
using WinterRose.ForgeVein.Networking.Validation;
using WinterRose.ForgeVein.Networking.Routing;

namespace WinterRose.ForgeVein.Networking.Application;

public interface INetworkServer
{
    ValueTask StartAsync(CancellationToken cancellationToken = default);
    ValueTask StopAsync(CancellationToken cancellationToken = default);
}

public interface INetworkClient
{
    ValueTask ConnectAsync(CancellationToken cancellationToken = default);
    ValueTask DisconnectAsync(CancellationToken cancellationToken = default);
}

public interface IPacketHandlerRegistry
{
    void Register<TPacket>(IPacketHandler<TPacket> handler);
    bool TryGetHandler<TPacket>(out IPacketHandler<TPacket>? handler);
}

public abstract class NetworkServerBase : INetworkServer
{
    protected NetworkServerBase(
        ISessionManager sessionManager,
        IPacketDispatcher dispatcher,
        IValidationPipeline validationPipeline,
        IRoutingStrategy routingStrategy,
        IPacketHandlerRegistry handlerRegistry)
    {
        SessionManager = sessionManager;
        Dispatcher = dispatcher;
        ValidationPipeline = validationPipeline;
        RoutingStrategy = routingStrategy;
        HandlerRegistry = handlerRegistry;
    }

    protected ISessionManager SessionManager { get; }
    protected IPacketDispatcher Dispatcher { get; }
    protected IValidationPipeline ValidationPipeline { get; }
    protected IRoutingStrategy RoutingStrategy { get; }
    protected IPacketHandlerRegistry HandlerRegistry { get; }

    public abstract ValueTask StartAsync(CancellationToken cancellationToken = default);
    public abstract ValueTask StopAsync(CancellationToken cancellationToken = default);
}

public abstract class NetworkClientBase : INetworkClient
{
    protected NetworkClientBase(ISessionManager sessionManager, IPacketDispatcher dispatcher)
    {
        SessionManager = sessionManager;
        Dispatcher = dispatcher;
    }

    protected ISessionManager SessionManager { get; }
    protected IPacketDispatcher Dispatcher { get; }

    public abstract ValueTask ConnectAsync(CancellationToken cancellationToken = default);
    public abstract ValueTask DisconnectAsync(CancellationToken cancellationToken = default);
}
