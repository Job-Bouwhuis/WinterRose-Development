using WinterRose.ForgeVein.Networking.Session;

namespace WinterRose.ForgeVein.Networking.Diagnostics;

public sealed class DefaultDiagnosticsProvider : IDiagnosticsProvider
{
    private readonly ISessionManager sessionManager;
    private DateTime lastUpdate = DateTime.MinValue;

    public DefaultDiagnosticsProvider(ISessionManager sessionManager)
    {
        this.sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
    }

    public IEnumerable<SessionDiagnostics> GetSessions()
    {
        var now = DateTime.UtcNow;

        return sessionManager.ActiveSessions.Select(session => new SessionDiagnostics(
            SessionId: session.SessionId,
            Statistics: session.Statistics,
            Metadata: session.Metadata,
            LastActivity: now
        )).ToList();
    }
}
