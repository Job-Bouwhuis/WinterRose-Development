using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer;

public class DebugReadStream : Stream
{
    public Stream BaseStream { get;  }

    public DebugReadStream(Stream innerStream)
    {
        BaseStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
    }

    public override bool CanRead => BaseStream.CanRead;
    public override bool CanSeek => BaseStream.CanSeek;
    public override bool CanWrite => BaseStream.CanWrite;
    public override long Length => BaseStream.Length;

    public override long Position
    {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
    }

    public override void Flush() => BaseStream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        int bytesRead = BaseStream.Read(buffer, offset, count);
        LogBytes(buffer, offset, bytesRead);
        return bytesRead;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        int bytesRead = await BaseStream.ReadAsync(buffer, offset, count, cancellationToken);
        LogBytes(buffer, offset, bytesRead);
        return bytesRead;
    }

    private void LogBytes(byte[] buffer, int offset, int count)
    {
        if (count <= 0) return;

        string ascii = Encoding.ASCII.GetString(buffer, offset, count);
        // Optional: sanitize non-printable chars if you want
        Console.WriteLine($"[DebugReadStream] Read {count} bytes: {ascii}");
    }

    public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

    public override void SetLength(long value) => BaseStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => BaseStream.Write(buffer, offset, count);
}