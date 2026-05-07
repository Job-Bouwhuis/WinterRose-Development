using WinterRose.ForgeVein.Networking.Session;

namespace WinterRose.ForgeVein.Networking.Diagnostics;

public sealed record SessionDiagnostics(Guid SessionId, SessionStatistics Statistics, SessionMetadata Metadata, DateTime LastActivity);

public interface IDiagnosticsProvider
{
    IEnumerable<SessionDiagnostics> GetSessions();
}
