using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace WinterRose.ForgeVein.Networking.Transport;

public sealed class UdpTransportListener : ITransportListener
{
    private readonly UdpClient udpClient;
    private readonly ConcurrentDictionary<Guid, EndPoint> pendingConnections = new();
    private readonly ConcurrentQueue<(Guid ConnectionId, EndPoint RemoteEndPoint)> connectionQueue = new();
    private readonly SemaphoreSlim connectionSemaphore = new(0);
    private CancellationTokenSource? cancellationTokenSource;
    private bool started;
    private bool disposed;

    public TransportProtocol Protocol => TransportProtocol.Udp;
    public EndPoint EndPoint => udpClient.Client.LocalEndPoint!;

    public UdpTransportListener(IPEndPoint endPoint)
    {
        if (endPoint == null)
            throw new ArgumentNullException(nameof(endPoint));

        udpClient = new UdpClient(endPoint);
    }

    public async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        if (disposed)
            throw new ObjectDisposedException(GetType().Name);

        if (started)
            return;

        started = true;
        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Start background receive loop
        _ = RunReceiveLoopAsync(cancellationTokenSource.Token);
        await ValueTask.CompletedTask.ConfigureAwait(false);
    }

    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        if (!started)
            return;

        started = false;
        cancellationTokenSource?.Cancel();
        await ValueTask.CompletedTask.ConfigureAwait(false);
    }

    public async ValueTask<ITransportConnection> AcceptAsync(CancellationToken cancellationToken = default)
    {
        if (disposed)
            throw new ObjectDisposedException(GetType().Name);

        if (!started)
            throw new InvalidOperationException("Listener has not been started.");

        // Wait for a new connection to be available
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await connectionSemaphore.WaitAsync(cts.Token).ConfigureAwait(false);

        if (connectionQueue.TryDequeue(out var connection))
        {
            var conn = new UdpTransportConnection(udpClient, connection.RemoteEndPoint);
            return conn;
        }

        throw new InvalidOperationException("Failed to dequeue connection.");
    }

    private async Task RunReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (started && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync(cancellationToken).ConfigureAwait(false);

                    // For UDP, each datagram from a new remote endpoint is a new "connection"
                    var connectionId = Guid.NewGuid();
                    pendingConnections.TryAdd(connectionId, result.RemoteEndPoint);
                    connectionQueue.Enqueue((connectionId, result.RemoteEndPoint));
                    connectionSemaphore.Release();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Log and continue
                }
            }
        }
        catch
        {
            // Ignore errors during shutdown
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
            return;

        disposed = true;

        try
        {
            await StopAsync().ConfigureAwait(false);
            udpClient?.Dispose();
            cancellationTokenSource?.Dispose();
            connectionSemaphore?.Dispose();
        }
        catch
        {
            // Ignore disposal errors
        }
    }
}
