using System.Collections.Concurrent;
using WinterRose.ForgeVein.Networking.Transport;

namespace WinterRose.ForgeVein.Networking.Session;

public sealed class DefaultSessionManager : ISessionManager, IAsyncDisposable
{
    private readonly ConcurrentDictionary<Guid, ISessionConnection> sessions = new();
    private bool disposed;

    public IEnumerable<ISessionConnection> ActiveSessions => sessions.Values;

    public ValueTask<ISessionConnection> CreateSessionAsync(ITransportConnection transport, CancellationToken cancellationToken = default)
    {
        if (disposed)
            throw new ObjectDisposedException(GetType().Name);

        if (transport == null)
            throw new ArgumentNullException(nameof(transport));

        var sessionId = Guid.NewGuid();
        var metadata = new SessionMetadata(
            ConnectedAt: DateTime.UtcNow,
            Username: null,
            Items: new Dictionary<string, object?>()
        );

        var session = new DefaultSessionConnection(sessionId, transport, metadata);

        if (!sessions.TryAdd(sessionId, session))
            throw new InvalidOperationException($"Failed to add session {sessionId}.");

        return ValueTask.FromResult<ISessionConnection>(session);
    }

    public async ValueTask CloseSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        if (sessions.TryRemove(sessionId, out var session))
        {
            try
            {
                await session.CloseAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Log error but don't throw
            }
        }
    }

    public bool TryGetSession(Guid sessionId, out ISessionConnection session)
    {
        return sessions.TryGetValue(sessionId, out session!);
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
            return;

        disposed = true;

        try
        {
            var sessionIds = sessions.Keys.ToList();
            foreach (var sessionId in sessionIds)
            {
                await CloseSessionAsync(sessionId).ConfigureAwait(false);
            }

            sessions.Clear();
        }
        catch
        {
            // Ignore disposal errors
        }
    }
}
