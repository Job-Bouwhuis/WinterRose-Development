using WinterRose.ForgeVein.Networking.Packets;
using WinterRose.ForgeVein.Networking.Session;

namespace WinterRose.ForgeVein.Networking.Routing;

public sealed class DefaultRoutingDecision : IRoutingDecision
{
    public RouteDescriptor Route { get; }

    public DefaultRoutingDecision(RouteDescriptor route)
    {
        Route = route;
    }
}

public sealed class DefaultRoutingStrategy : IRoutingStrategy
{
    public IRoutingDecision Decide(ISessionConnection session, IPacketDescriptor packetDescriptor, ReadOnlyMemory<byte> payload)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (packetDescriptor == null)
            throw new ArgumentNullException(nameof(packetDescriptor));

        // Default routing strategy: all packets are handled directly on the server
        var route = new RouteDescriptor(
            Kind: RouteKind.Direct,
            DestinationSession: null,
            StreamId: null,
            ClusterService: null
        );

        return new DefaultRoutingDecision(route);
    }
}
