using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace WinterRose;

/// <summary>
/// Enumerates over a file in chunks. Default chunk size is 1024 bytes
/// </summary>
public sealed class FileEnumerator(FileStream file, int chunkSize = 1024) : IEnumerator<byte[]>, IClearDisposable
{
    byte[] currentChunk;

    public FileEnumerator(string path, int chunkSize = 1024) : this(File.OpenRead(path), chunkSize) { }

    public static implicit operator FileEnumerator(FileStream stream) => new(stream);

    public byte[] Current => currentChunk;
    public int LastReadCount { get; private set; }
    object IEnumerator.Current => Current;

    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Closes the file.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;

        file.Close();
        file.Dispose();
    }

    /// <summary>
    /// Gets the next chunk of the file
    /// </summary>
    /// <returns>The next chunk</returns>
    public bool MoveNext()
    {
        currentChunk = new byte[chunkSize];
        LastReadCount = file.Read(Current, 0, currentChunk.Length);
        return LastReadCount > 0;
    }

    /// <summary>
    /// Resets the enumerator to the start of the file
    /// </summary>
    public void Reset() => file.Position = 0;

    /// <summary>
    /// Gets this instance.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<byte[]> GetEnumerator() => this;
}
/// <summary>
/// Provides a single extension method to get a <see cref="FileEnumerator"/> from a <see cref="FileStream"/>
/// </summary>
public static class FileEnumeratorExtension
{
    public static FileEnumerator GetEnumerator(this FileStream file) => new(file);
}
