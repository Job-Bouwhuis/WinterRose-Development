using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.Diff;

public class FileView : IDisposable
{
    private readonly FileStream stream;

    private readonly byte[] buffer;
    private long bufferStart;
    private int bufferLength;

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
