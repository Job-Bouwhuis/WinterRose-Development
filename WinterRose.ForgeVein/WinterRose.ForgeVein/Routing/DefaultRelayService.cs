using WinterRose.ForgeVein.Networking.Session;

namespace WinterRose.ForgeVein.Networking.Routing;

public sealed class DefaultRelayService : IRelayService
{
    private readonly ISessionManager sessionManager;

    public DefaultRelayService(ISessionManager sessionManager)
    {
        this.sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
    }

    public async ValueTask RelayAsync(RouteDescriptor route, ISessionConnection sender, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
    {
        if (route == null)
            throw new ArgumentNullException(nameof(route));

        if (route.DestinationSession == null)
            throw new ArgumentException("Route must specify a destination session for relay.", nameof(route));

        if (!sessionManager.TryGetSession(route.DestinationSession.Value, out var destinationSession))
            throw new InvalidOperationException($"Destination session {route.DestinationSession} not found.");

        try
        {
            // TODO: Add proper reliability mode handling based on packet metadata
            await destinationSession.SendAsync(payload, ReliabilityMode.Reliable, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to relay packet to destination session.", ex);
        }
    }
}
