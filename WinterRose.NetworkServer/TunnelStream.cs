using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.IO.IsolatedStorage;

namespace WinterRose.NetworkServer;

public class TunnelStream : Stream
{
    private readonly NetworkStream remote;
    private readonly MemoryStream writeBuffer = new();
    private readonly SemaphoreSlim writeLock = new(1, 1);

    private readonly MemoryStream readBuffer = new();
    private readonly SemaphoreSlim readLock = new(1, 1);
    private readonly byte[] readTempBuffer = new byte[8192];

    private bool tunnelEndDetected = false;
    const string TUNNEL_END = "<TUNNEL.END>";
    private bool closedByRemote = false;

    private bool isClosed = false;

    public TunnelStream(NetworkConnection connection)
    {
        remote = connection.GetStream();
        _ = Task.Run(BackgroundReadLoop);
    }

    public bool Closed => isClosed;

    public override bool CanRead => !isClosed;
    public override bool CanSeek => false;
    public override bool CanWrite => !isClosed;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public Action OnClosed { get; set; } = delegate { };

    private async Task BackgroundReadLoop()
    {
        try
        {
            List<byte> messageBuffer = [];
            while (!tunnelEndDetected)
            {
                await Task.Delay(10);

                int read = await remote.ReadAsync(readTempBuffer, 0, readTempBuffer.Length);
                if (read == 0)
                {
                    tunnelEndDetected = true;
                    break;
                }

                messageBuffer.AddRange(readTempBuffer.Take(read));
                int messageendmarker = Encoding.UTF8.GetString(messageBuffer.ToArray()).IndexOf("<END>");
                if (messageendmarker == -1)
                {
                    continue;
                }

                messageBuffer.RemoveRange(messageendmarker, 5);

                await readLock.WaitAsync();
                try
                {
                    readBuffer.Write(messageBuffer.ToArray(), 0, messageBuffer.Count);

                    // Check for tunnel end marker in the buffer
                    string bufferString = Encoding.UTF8.GetString(readBuffer.GetBuffer(), 0, (int)readBuffer.Length);
                    int markerIndex = bufferString.IndexOf(TUNNEL_END, StringComparison.Ordinal);
                    if (markerIndex != -1)
                    {
                        readBuffer.SetLength(markerIndex);
                        tunnelEndDetected = true;
                        closedByRemote = true;
                        break;
                    }
                }
                catch(Exception ex)
                {

                }
                finally
                {
                    readLock.Release();
                }
            }
        }
        catch
        {
            tunnelEndDetected = true;
        }
    }


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

    public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count, new()).GetAwaiter().GetResult();

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        while (true)
        {
            if (isClosed && readBuffer.Length == 0)
                return 0;

            await readLock.WaitAsync(cancellationToken);
            try
            {
                if (readBuffer.Length > 0)
                {
                    readBuffer.Position = 0;
                    int bytesToRead = (int)Math.Min(count, readBuffer.Length);
                    int bytesRead = readBuffer.Read(buffer, offset, bytesToRead);

                    // Shift remaining data to the front
                    var remaining = readBuffer.Length - readBuffer.Position;
                    var leftover = readBuffer.GetBuffer().AsSpan((int)readBuffer.Position, (int)remaining).ToArray();

                    readBuffer.SetLength(0);
                    readBuffer.Position = 0;
                    readBuffer.Write(leftover, 0, leftover.Length);

                    if (leftover.Length == 0)
                    {
                        isClosed = true;
                    }
                    return bytesRead;
                }
            }
            finally
            {
                readLock.Release();
            }

            await Task.Delay(2, cancellationToken); // small wait to avoid hot-looping
        }
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

        if (!closedByRemote)
        {
            remote.Write(System.Text.Encoding.UTF8.GetBytes("<TUNNEL.END>"));
            remote.Flush();
            isClosed = true;
        }

    }

    public void DoWrite(string data) => DoWrite(Encoding.UTF8.GetBytes(data));

    private void DoWrite(byte[] data) => DoWrite(data, data.Length);

    readonly byte[] endMarker = [60, 69, 78, 68, 62];

    private void DoWrite(byte[] data, int length)
    {
        Span<byte> bytes = stackalloc byte[data.Length + 5];

        data.CopyTo(bytes);

        for (int i = 0; i < endMarker.Length; i++)
            bytes[data.Length + i] = endMarker[i];

        ReadOnlySpan<byte> rbytes = bytes;
        remote.Write(rbytes);
    }

    public async Task DoWriteAsync(string data) =>
        await DoWriteAsync(Encoding.UTF8.GetBytes(data));

    private async Task DoWriteAsync(byte[] data) =>
        await DoWriteAsync(data, data.Length);

    private async Task DoWriteAsync(byte[] data, int length)
    {
        await Task.Run(() => DoWrite(data, length));
    }
        

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

