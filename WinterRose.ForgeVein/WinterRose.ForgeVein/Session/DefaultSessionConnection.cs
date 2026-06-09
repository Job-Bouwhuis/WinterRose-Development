using WinterRose.ForgeVein.Networking.Transport;

namespace WinterRose.ForgeVein.Networking.Session;

public sealed class DefaultSessionConnection : ISessionConnection
{
    private readonly ITransportConnection transport;
    private SessionState state;
    private SessionStatistics statistics;
    private readonly object statisticsLock = new();

    public Guid SessionId { get; }
    public SessionState State => state;
    public SessionMetadata Metadata { get; }
    public SessionStatistics Statistics 
    { 
        get 
        { 
            lock (statisticsLock) 
            { 
                return statistics; 
            } 
        } 
    }
    public ITransportConnection Transport => transport;

    public DefaultSessionConnection(Guid sessionId, ITransportConnection transportConnection, SessionMetadata metadata)
    {
        SessionId = sessionId;
        transport = transportConnection ?? throw new ArgumentNullException(nameof(transportConnection));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        state = SessionState.Handshaking;
        statistics = new SessionStatistics(0, 0, 0, 0);
    }

    public async ValueTask<AcknowledgementResult> SendAsync(ReadOnlyMemory<byte> payload, ReliabilityMode reliability, CancellationToken cancellationToken = default)
    {
        if (state == SessionState.Closing || state == SessionState.Closed)
            throw new InvalidOperationException($"Cannot send on session in state {state}.");

        try
        {
            // For TCP, all data is reliable by default
            // For UDP, unreliable and HotPath skip acknowledgement tracking
            if (transport.Protocol == TransportProtocol.Tcp || reliability == ReliabilityMode.Reliable)
            {
                var acknowledgementId = Guid.NewGuid();
                await transport.SendAsync(payload, cancellationToken).ConfigureAwait(false);

                lock (statisticsLock)
                {
                    statistics = statistics with 
                    { 
                        BytesSent = statistics.BytesSent + payload.Length,
                        PacketsSent = statistics.PacketsSent + 1
                    };
                }

                return new AcknowledgementResult(true, acknowledgementId);
            }
            else
            {
                // Unreliable or HotPath
                await transport.SendAsync(payload, cancellationToken).ConfigureAwait(false);

                lock (statisticsLock)
                {
                    statistics = statistics with 
                    { 
                        BytesSent = statistics.BytesSent + payload.Length,
                        PacketsSent = statistics.PacketsSent + 1
                    };
                }

                return new AcknowledgementResult(false, null);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to send data on session.", ex);
        }
    }

    public async ValueTask<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        if (state == SessionState.Closed)
            throw new InvalidOperationException("Cannot receive on closed session.");

        try
        {
            var result = await transport.ReceiveAsync(cancellationToken).ConfigureAwait(false);

            lock (statisticsLock)
            {
                statistics = statistics with 
                { 
                    BytesReceived = statistics.BytesReceived + result.Payload.Length,
                    PacketsReceived = statistics.PacketsReceived + 1
                };
            }

            return result.Payload;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to receive data on session.", ex);
        }
    }

    public async ValueTask CloseAsync(CancellationToken cancellationToken = default)
    {
        if (state == SessionState.Closed)
            return;

        try
        {
            state = SessionState.Closing;
            await transport.CloseAsync(cancellationToken).ConfigureAwait(false);
            state = SessionState.Closed;
        }
        catch
        {
            state = SessionState.Closed;
        }
    }

    internal void SetState(SessionState newState)
    {
        state = newState;
    }
}
