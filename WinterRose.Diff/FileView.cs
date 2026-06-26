using System.Security.Cryptography;

namespace WinterRose.Diff;

public class FileView : IDisposable
{
    private readonly FileStream stream;

    private readonly byte[] buffer;
    private long bufferStart;
    private int bufferLength;

    public static implicit operator FileStream(FileView view) => view.stream;

    public string ComputeSha256()
    {
        var location = stream.Position;
        stream.Position = 0;
        using var sha256 = SHA256.Create();

        byte[] hash = sha256.ComputeHash(stream);

        stream.Position = location;

        return Convert.ToHexString(hash);
    }

    public FileView(string path)
    {
        stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            1 << 20,
            FileOptions.SequentialScan);

        buffer = new byte[8 * 1024 * 1024];
    }

    public byte this[long index]
    {
        get
        {
            if (index < bufferStart || index >= bufferStart + bufferLength)
                FillBuffer(index);

            return buffer[index - bufferStart];
        }
    }

    private void FillBuffer(long index)
    {
        bufferStart = index;

        stream.Seek(index, SeekOrigin.Begin);
        bufferLength = stream.Read(buffer, 0, buffer.Length);
    }

    public void Dispose() => stream.Dispose();

    public long Length => stream.Length;
}
