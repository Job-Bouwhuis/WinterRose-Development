using System.Net;
using System.Net.Sockets;

namespace WinterRose.ForgeVein.Networking.Transport;

public sealed class UdpTransportConnection : ITransportConnection
{
    private readonly UdpClient udpClient;
    private readonly Guid connectionId;
    private EndPoint? remoteEndPoint;
    private bool disposed;

    public Guid ConnectionId => connectionId;
    public TransportProtocol Protocol => TransportProtocol.Udp;
    public EndPoint? RemoteEndPoint => remoteEndPoint;

    public UdpTransportConnection(UdpClient client, EndPoint? remoteEndPoint = null)
    {
        connectionId = Guid.NewGuid();
        udpClient = client ?? throw new ArgumentNullException(nameof(client));
        this.remoteEndPoint = remoteEndPoint;
    }

    public async ValueTask SendAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
    {
        if (disposed)
            throw new ObjectDisposedException(GetType().Name);

        if (udpClient == null)
            throw new InvalidOperationException("UDP client is not available.");

        if (remoteEndPoint == null)
            throw new InvalidOperationException("Remote endpoint not set.");

        try
        {
            // Convert ReadOnlyMemory<byte> to byte[]
            byte[] buffer = payload.ToArray();
            await udpClient.SendAsync(buffer, buffer.Length, (IPEndPoint)remoteEndPoint).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to send UDP datagram.", ex);
        }
    }

    public async ValueTask<TransportReceiveResult> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        if (disposed)
            throw new ObjectDisposedException(GetType().Name);

        if (udpClient == null)
            throw new InvalidOperationException("UDP client is not available.");

        try
        {
            var result = await udpClient.ReceiveAsync(cancellationToken).ConfigureAwait(false);
            remoteEndPoint = result.RemoteEndPoint;
            return new TransportReceiveResult(new ReadOnlyMemory<byte>(result.Buffer), result.RemoteEndPoint, DateTime.UtcNow);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to receive UDP datagram.", ex);
        }
    }

    public async ValueTask CloseAsync(CancellationToken cancellationToken = default)
    {
        if (disposed)
            return;

        disposed = true;

        try
        {
            udpClient?.Dispose();
        }
        catch
        {
            // Ignore disposal errors
        }

        await ValueTask.CompletedTask.ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait(false);
    }
}
