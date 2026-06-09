using System.Net;
using System.Net.Sockets;

namespace WinterRose.ForgeVein.Networking.Transport;

public sealed class TcpTransportListener : ITransportListener
{
    private readonly TcpListener tcpListener;
    private CancellationTokenSource? cancellationTokenSource;
    private bool started;
    private bool disposed;

    public TransportProtocol Protocol => TransportProtocol.Tcp;
    public EndPoint EndPoint => tcpListener.LocalEndpoint;

    public TcpTransportListener(IPEndPoint endPoint)
    {
        tcpListener = new TcpListener(endPoint ?? throw new ArgumentNullException(nameof(endPoint)));
    }

    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        if (disposed)
            throw new ObjectDisposedException(GetType().Name);

        if (started)
            return ValueTask.CompletedTask;

        try
        {
            tcpListener.Start();
            started = true;
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            return ValueTask.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to start TCP listener.", ex);
        }
    }

    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        if (!started)
            return;

        try
        {
            started = false;
            cancellationTokenSource?.Cancel();
            tcpListener?.Stop();
            await ValueTask.CompletedTask.ConfigureAwait(false);
        }
        catch
        {
            // Ignore stop errors
        }
    }

    public async ValueTask<ITransportConnection> AcceptAsync(CancellationToken cancellationToken = default)
    {
        if (disposed)
            throw new ObjectDisposedException(GetType().Name);

        if (!started)
            throw new InvalidOperationException("Listener has not been started.");

        try
        {
            var client = await tcpListener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
            return new TcpTransportConnection(client);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to accept TCP connection.", ex);
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
            tcpListener?.Stop();
            cancellationTokenSource?.Dispose();
        }
        catch
        {
            // Ignore disposal errors
        }
    }
}
