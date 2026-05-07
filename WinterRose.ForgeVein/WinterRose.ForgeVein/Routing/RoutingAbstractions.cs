using WinterRose.ForgeVein.Networking.Packets;
using WinterRose.ForgeVein.Networking.Session;

namespace WinterRose.ForgeVein.Networking.Routing;

public enum RouteKind
{
    Direct,
    Relay,
    Stream,
    Delegation
}

public sealed record RouteDescriptor(RouteKind Kind, Guid? DestinationSession, string? StreamId, string? ClusterService);

public interface IRoutingDecision
{
    RouteDescriptor Route { get; }
}

public interface IRoutingStrategy
{
    IRoutingDecision Decide(ISessionConnection session, IPacketDescriptor packetDescriptor, ReadOnlyMemory<byte> payload);
}

public interface IRelayService
{
    ValueTask RelayAsync(RouteDescriptor route, ISessionConnection sender, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default);
}

public interface IStreamRouter
{
    ValueTask OpenStreamAsync(string streamId, ISessionConnection initiator, ISessionConnection target, CancellationToken cancellationToken = default);
    ValueTask CloseStreamAsync(string streamId, ISessionConnection requester, CancellationToken cancellationToken = default);
    ValueTask RelayStreamAsync(string streamId, ISessionConnection sender, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default);
}

public interface IClusterDelegationService
{
    ValueTask RequestDelegationAsync(Guid sessionId, string targetService, CancellationToken cancellationToken = default);
    ValueTask NotifyIncomingDelegationAsync(Guid sessionId, string originService, CancellationToken cancellationToken = default);
    ValueTask ConfirmDelegationAsync(Guid sessionId, string originService, CancellationToken cancellationToken = default);
    ValueTask InstructClientReconnectAsync(Guid sessionId, string targetService, CancellationToken cancellationToken = default);
}
