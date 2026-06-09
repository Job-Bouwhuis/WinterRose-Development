using System.Net;
using System.Net.Sockets;

namespace WinterRose.ForgeVein.Networking.Transport;

public sealed class TcpTransportConnection : ITransportConnection
{
    private readonly NetworkStream networkStream;
    private readonly TcpClient tcpClient;
    private readonly Guid connectionId;
    private bool disposed;

    public Guid ConnectionId => connectionId;
    public TransportProtocol Protocol => TransportProtocol.Tcp;
    public EndPoint? RemoteEndPoint => tcpClient.Client.RemoteEndPoint;

    public TcpTransportConnection(TcpClient client)
    {
        connectionId = Guid.NewGuid();
        tcpClient = client ?? throw new ArgumentNullException(nameof(client));
        networkStream = client.GetStream();
    }

    public async ValueTask SendAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
    {
        if (disposed)
            throw new ObjectDisposedException(GetType().Name);

        if (networkStream == null)
            throw new InvalidOperationException("Network stream is not available.");

        await networkStream.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
        await networkStream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<TransportReceiveResult> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        if (disposed)
            throw new ObjectDisposedException(GetType().Name);

        if (networkStream == null)
            throw new InvalidOperationException("Network stream is not available.");

        // Read frame length (4 bytes, big-endian)
        byte[] lengthBuffer = new byte[4];
        int bytesRead = await networkStream.ReadAsync(lengthBuffer, 0, 4, cancellationToken).ConfigureAwait(false);

        if (bytesRead == 0)
            return new TransportReceiveResult(ReadOnlyMemory<byte>.Empty, RemoteEndPoint, DateTime.UtcNow);

        if (bytesRead < 4)
            throw new InvalidOperationException("Incomplete frame length received.");

        int frameLength = BitConverter.ToInt32(new byte[] { lengthBuffer[3], lengthBuffer[2], lengthBuffer[1], lengthBuffer[0] }, 0);

        if (frameLength <= 0 || frameLength > 1024 * 1024) // 1MB max frame
            throw new InvalidOperationException($"Invalid frame length: {frameLength}");

        // Read frame data
        byte[] frameBuffer = new byte[frameLength];
        int totalBytesRead = 0;

        while (totalBytesRead < frameLength)
        {
            int bytesToRead = frameLength - totalBytesRead;
            bytesRead = await networkStream.ReadAsync(frameBuffer, totalBytesRead, bytesToRead, cancellationToken).ConfigureAwait(false);

            if (bytesRead == 0)
                throw new InvalidOperationException("Connection closed while reading frame.");

            totalBytesRead += bytesRead;
        }

        return new TransportReceiveResult(new ReadOnlyMemory<byte>(frameBuffer), RemoteEndPoint, DateTime.UtcNow);
    }

    public async ValueTask CloseAsync(CancellationToken cancellationToken = default)
    {
        if (disposed)
            return;

        disposed = true;

        try
        {
            networkStream?.Dispose();
            tcpClient?.Close();
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
