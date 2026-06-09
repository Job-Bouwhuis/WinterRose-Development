using System.Collections.Concurrent;
using WinterRose.ForgeVein.Networking.Session;

namespace WinterRose.ForgeVein.Networking.Routing;

public sealed class StreamPair
{
    public string StreamId { get; }
    public ISessionConnection InitiatorSession { get; }
    public ISessionConnection TargetSession { get; }

    public StreamPair(string streamId, ISessionConnection initiatorSession, ISessionConnection targetSession)
    {
        StreamId = streamId ?? throw new ArgumentNullException(nameof(streamId));
        InitiatorSession = initiatorSession ?? throw new ArgumentNullException(nameof(initiatorSession));
        TargetSession = targetSession ?? throw new ArgumentNullException(nameof(targetSession));
    }
}

public sealed class DefaultStreamRouter : IStreamRouter
{
    private readonly ConcurrentDictionary<string, StreamPair> streams = new();

    public ValueTask OpenStreamAsync(string streamId, ISessionConnection initiator, ISessionConnection target, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty.", nameof(streamId));

        if (initiator == null)
            throw new ArgumentNullException(nameof(initiator));

        if (target == null)
            throw new ArgumentNullException(nameof(target));

        var pair = new StreamPair(streamId, initiator, target);
        if (!streams.TryAdd(streamId, pair))
            throw new InvalidOperationException($"Stream {streamId} is already open.");

        return ValueTask.CompletedTask;
    }

    public ValueTask CloseStreamAsync(string streamId, ISessionConnection requester, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty.", nameof(streamId));

        if (requester == null)
            throw new ArgumentNullException(nameof(requester));

        if (streams.TryRemove(streamId, out var stream))
        {
            // Verify that the requester is part of the stream
            if (stream.InitiatorSession.SessionId != requester.SessionId && 
                stream.TargetSession.SessionId != requester.SessionId)
            {
                // Add stream back if requester is not part of it
                streams.TryAdd(streamId, stream);
                throw new InvalidOperationException("Requester is not part of this stream.");
            }
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask RelayStreamAsync(string streamId, ISessionConnection sender, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId))
            throw new ArgumentException("Stream ID cannot be null or empty.", nameof(streamId));

        if (sender == null)
            throw new ArgumentNullException(nameof(sender));

        if (!streams.TryGetValue(streamId, out var stream))
            throw new InvalidOperationException($"Stream {streamId} not found.");

        ISessionConnection recipient = sender.SessionId == stream.InitiatorSession.SessionId 
            ? stream.TargetSession 
            : stream.InitiatorSession;

        try
        {
            // TODO: Add proper reliability mode handling based on packet metadata
            await recipient.SendAsync(payload, ReliabilityMode.Reliable, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to relay stream data on stream {streamId}.", ex);
        }
    }
}
