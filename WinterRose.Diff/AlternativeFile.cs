namespace WinterRose.Diff;

/// <summary>
/// Defines an alternative file for the diff operation, if a given file fails to patch, this class can act as a replacement bridge
/// </summary>
public class AlternativeFile : IDisposable, IAsyncDisposable
{
    private readonly Stream source;
    private readonly string hash;
    private bool disposed = false;

    /// <summary>
    /// Creates a new instance of the <see cref="AlternativeFile"/> class
    /// </summary>
    /// <param name="source"><see cref="AlternativeFile"/> owns the stream</param>
    /// <param name="hash">A hash of the data in <paramref name="source"/></param>
    public AlternativeFile(Stream source, string hash)
    {
        this.source = source;
        this.hash = hash;
    }

    public string Hash => hash;
    public string HashAlgorithm => "SHA1";

    /// <summary>
    /// Copies the content of this alternative file to the <paramref name="destination"/>
    /// </summary>
    /// <param name="destination"></param>
    public void CopyTo(Stream destination)
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(AlternativeFile));
        source.CopyTo(destination);
        if (source.CanSeek)
            source.Position = 0;
    }

    /// <summary>
    /// Copies the content of this alternative file to the <paramref name="destination"/>
    /// </summary>
    /// <param name="destination"></param>
    public async Task CopyToAsync(Stream destination)
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(AlternativeFile));
        await source.CopyToAsync(destination);
        if (source.CanSeek)
            source.Position = 0;
    }

    public void Dispose()
    {
        if (disposed)
            return;
        
        disposed = true;
        source.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
            return;
        
        disposed = true;
        await source.DisposeAsync();
    }
}