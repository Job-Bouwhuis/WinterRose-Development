using WinterRose.ForgeVein.Networking.Transport;

namespace WinterRose.ForgeVein.Networking.Session;

public enum SessionState
{
    Handshaking,
    Authenticated,
    Active,
    Closing,
    Closed
}

public enum ReliabilityMode
{
    Reliable,
    Unreliable,
    HotPath
}

public sealed record SessionMetadata(DateTime ConnectedAt, string? Username, IDictionary<string, object?> Items);

public sealed record SessionStatistics(long BytesSent, long BytesReceived, long PacketsSent, long PacketsReceived);

public sealed record AcknowledgementResult(bool WasAcknowledged, Guid? AcknowledgementId);

public interface ISessionConnection
{
    Guid SessionId { get; }
    SessionState State { get; }
    SessionMetadata Metadata { get; }
    SessionStatistics Statistics { get; }
    ITransportConnection Transport { get; }
    ValueTask<AcknowledgementResult> SendAsync(ReadOnlyMemory<byte> payload, ReliabilityMode reliability, CancellationToken cancellationToken = default);
    ValueTask<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken cancellationToken = default);
    ValueTask CloseAsync(CancellationToken cancellationToken = default);
}

public interface ISessionManager
{
    IEnumerable<ISessionConnection> ActiveSessions { get; }
    ValueTask<ISessionConnection> CreateSessionAsync(ITransportConnection transport, CancellationToken cancellationToken = default);
    ValueTask CloseSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    bool TryGetSession(Guid sessionId, out ISessionConnection session);
}

public interface IReliabilityHandler
{
    ValueTask<AcknowledgementResult> SendAsync(ISessionConnection session, ReadOnlyMemory<byte> payload, ReliabilityMode reliability, CancellationToken cancellationToken = default);
    ValueTask<ReadOnlyMemory<byte>> ReceiveAsync(ISessionConnection session, CancellationToken cancellationToken = default);
}

public interface IEncryptionStage
{
    ReadOnlyMemory<byte> Encrypt(ReadOnlyMemory<byte> payload, ISessionConnection session);
    ReadOnlyMemory<byte> Decrypt(ReadOnlyMemory<byte> payload, ISessionConnection session);
}
