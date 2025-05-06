using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class TunnelStream : Stream
{
    private readonly NetworkConnection connection;
    private readonly NetworkStream remote;
    private readonly Guid remoteIdentifier;
    private readonly MemoryStream writeBuffer = new();
    private readonly SemaphoreSlim writeLock = new(1, 1);
    private readonly Func<Task<byte[]?>> waitForTunnelData;

    private bool isClosed = false;

    public TunnelStream(NetworkConnection connection, Guid remoteIdentifier, Func<Task<byte[]?>> waitForTunnelData)
    {
        this.connection = connection;
        remote = connection.GetStream();
        this.remoteIdentifier = remoteIdentifier;
        this.waitForTunnelData = waitForTunnelData;
    }

    public override bool CanRead => !isClosed;
    public override bool CanSeek => false;
    public override bool CanWrite => !isClosed;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override void Flush()
    {
        FlushAsync().GetAwaiter().GetResult();
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        await writeLock.WaitAsync(cancellationToken);
        try
        {
            if (writeBuffer.Length == 0 || isClosed) return;

            byte[] payload = writeBuffer.ToArray();
            writeBuffer.SetLength(0);

            // This is the raw tunnel-forward message to the server
            await DoWriteAsync(payload);
        }
        finally
        {
            writeLock.Release();
        }
    }

    public override int Read(byte[] buffer, int offset, int count) => Read(buffer, offset, count);

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (isClosed)
            throw new IOException("Tunnel is closed");

        byte[]? incoming = await waitForTunnelData();

        if (incoming == null || incoming.Length == 0)
        {
            isClosed = true;
            return 0;
        }

        int bytesToCopy = Math.Min(count, incoming.Length);
        Array.Copy(incoming, 0, buffer, offset, bytesToCopy);
        return bytesToCopy;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        WriteAsync(buffer, offset, count).GetAwaiter().GetResult();
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (isClosed)
            throw new IOException("Tunnel is closed");

        await writeLock.WaitAsync(cancellationToken);
        try
        {
            writeBuffer.Write(buffer, offset, count);
        }
        finally
        {
            writeLock.Release();
        }
    }

    public override void Close()
    {
        if (isClosed) return;

        isClosed = true;
        remote.Write(System.Text.Encoding.UTF8.GetBytes("<TUNNEL.END>"));
        base.Close();
    }

    private void DoWrite(string data) => DoWrite(Encoding.UTF8.GetBytes(data));

    private void DoWrite(byte[] data) => DoWrite(data, data.Length);

    private void DoWrite(byte[] data, int length) => remote.Write(data, 0, length);

    private async Task DoWriteAsync(string data) =>
    await DoWriteAsync(Encoding.UTF8.GetBytes(data));

    private async Task DoWriteAsync(byte[] data) =>
        await DoWriteAsync(data, data.Length);

    private async Task DoWriteAsync(byte[] data, int length) =>
        await remote.WriteAsync(data, 0, length);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            writeBuffer.Dispose();
            writeLock.Dispose();
        }

        base.Dispose(disposing);
    }

    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException();

    public override void SetLength(long value) =>
        throw new NotSupportedException();
}

