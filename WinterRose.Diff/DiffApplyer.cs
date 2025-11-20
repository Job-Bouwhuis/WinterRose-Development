using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.Diff;
using WinterRose.WinterForgeSerializing;

public class DiffApplyer
{
    public const int BUFFER_SIZE = 8192;

    public async Task ApplyDirectoryDiff(DirectoryDiff dirDiff, string destinationDirectory, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dirDiff);
        destinationDirectory ??= Directory.GetCurrentDirectory();

        List<FileDiff> files = new List<FileDiff>();
        foreach (var op in dirDiff.FileDiffs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(op.Value.Operations.Count is 1 && op.Value.Operations.First() is DiffEngine.DeleteFile delFile)
            {
                string fullPath = Path.Combine(destinationDirectory, op.Key);
                var deleteOp = new DiffEngine.DeleteFile()
                {
                    FullName = fullPath
                };
                await ApplyOpsFileBackedAsync(null, new DiffEngine.Op[] { deleteOp }, Stream.Null, cancellationToken);
            }
            if (op.Value.Operations.Count is 1 && op.Value.Operations.First() is DiffEngine.CreateFile createFile)
            {
                string fullPath = Path.Combine(destinationDirectory, op.Key);
                var createOp = new DiffEngine.CreateFile(createFile.Data)
                {
                    FullName = fullPath
                };
                await ApplyOpsFileBackedAsync(null, new DiffEngine.Op[] { createOp }, Stream.Null, cancellationToken);
            }
        }
    }

    public async Task ApplyFileDiffFrom(string filePath, Stream targetStream, string workingDirectory = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
        if (!File.Exists(filePath)) throw new FileNotFoundException("Diff operations file not found", filePath);

        var diffResult = FileDiff.Load(filePath);
        await ApplyFileDiffAsync(targetStream, diffResult, workingDirectory, cancellationToken);
    }

    public async Task ApplyFileDiffAsync(Stream targetStream, FileDiff ops, string workingDirectory = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(targetStream);
        ArgumentNullException.ThrowIfNull(ops);

        workingDirectory ??= Directory.GetCurrentDirectory();
        string tempOriginal = Path.Combine(workingDirectory, "diff_orig_" + Guid.NewGuid().ToString("N") + ".tmp");
        string tempResult = Path.Combine(workingDirectory, "diff_res_" + Guid.NewGuid().ToString("N") + ".tmp");

        try
        {
            CopyStreamToFileAsync(targetStream, tempOriginal);

            // 2) open tempOriginal and tempResult and apply ops (file-backed)
            using (var originalFs = new FileStream(tempOriginal, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, useAsync: true))
            using (var resultFs = new FileStream(tempResult, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE, useAsync: true))
            {
                var orderedOps = ops.Operations
                    .Where(o => o is DiffEngine.Insert || o is DiffEngine.Copy)
                    .OrderBy(o => o.NewOffset)
                    .ToArray();

                await ApplyOpsFileBackedAsync(originalFs, orderedOps, resultFs, cancellationToken);
                await resultFs.FlushAsync(cancellationToken);
            }

            // 3) write result back into targetStream (overwrite)
            await CopyFileToStreamAsync(tempResult, targetStream, cancellationToken);
        }
        finally
        {
            TryDeleteTemp(tempOriginal);
            TryDeleteTemp(tempResult);
        }
    }

    private static void CopyStreamToFileAsync(Stream source, string path)
    {
        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE))
        {
            try
            {
                SeekIfPossible(source, 0);
                source.CopyTo(fs);
                fs.Flush();
            }
            catch (Exception ex)
            {
                throw new IOException("Failed to copy source stream to temporary file.", ex);
            }
        }
    }

    private static async Task CopyFileToStreamAsync(string filePath, Stream destination, CancellationToken cancellationToken)
    {
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        if (!File.Exists(filePath)) throw new FileNotFoundException("Result file missing", filePath);

        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, useAsync: true))
        {
            // destination must be writable for us to copy back
            if (!destination.CanWrite)
                throw new InvalidOperationException("Target stream is not writable; cannot overwrite with patched result.");

            // If destination is seekable, truncate and rewind to behave like "overwrite"
            if (destination.CanSeek)
            {
                try
                {
                    destination.SetLength(0);
                    destination.Seek(0, SeekOrigin.Begin);
                }
                catch
                {
                    // If truncation fails for any reason, fall back to writing from current position (best-effort)
                }
            }

            await fs.CopyToAsync(destination, BUFFER_SIZE, cancellationToken);

            if (destination.CanSeek)
            {
                try { destination.Seek(0, SeekOrigin.Begin); } catch { /* best-effort */ }
            }

            await destination.FlushAsync(cancellationToken);
        }
    }

    private static void TryDeleteTemp(string path)
    {
        try { if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path); } catch { }
    }

    // --------------------------------------------------------------------
    // Core file-backed apply logic (reads original FileStream and writes resulting bytes to output stream)
    // --------------------------------------------------------------------
    private async Task ApplyOpsFileBackedAsync(FileStream original, IEnumerable<DiffEngine.Op> orderedOps, Stream output, CancellationToken cancellationToken)
    {
        long newCursor = 0;
        byte[] buffer = new byte[BUFFER_SIZE];

        foreach (var op in orderedOps)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // If there's a gap in the new file before this op, write zeros (best-effort to preserve offsets).
            if (op.NewOffset > newCursor)
            {
                long gap = op.NewOffset - newCursor;
                while (gap > 0)
                {
                    int chunk = (int)Math.Min(BUFFER_SIZE, gap);
                    await output.WriteAsync(new byte[chunk], 0, chunk, cancellationToken);
                    newCursor += chunk;
                    gap -= chunk;
                }
            }

            if (op is DiffEngine.Insert ins)
            {
                // If insert overlaps with already-written region, skip the overlapping prefix.
                long overlap = Math.Max(0, newCursor - ins.NewOffset);
                int startIndex = (int)overlap;
                int toWrite = ins.Data.Length - startIndex;
                if (toWrite > 0)
                {
                    await output.WriteAsync(ins.Data, startIndex, toWrite, cancellationToken);
                    newCursor += toWrite;
                }
            }
            else if (op is DiffEngine.Copy cp)
            {
                long remaining = cp.Length;
                long srcPos = cp.OldOffset;

                // If copy overlaps with already-written region, skip the overlapping prefix.
                long skip = Math.Max(0, newCursor - cp.NewOffset);
                if (skip > 0)
                {
                    srcPos += skip;
                    remaining -= skip;
                }

                while (remaining > 0)
                {
                    int toRead = (int)Math.Min(BUFFER_SIZE, remaining);
                    original.Seek(srcPos, SeekOrigin.Begin);
                    int read = await original.ReadAsync(buffer, 0, toRead, cancellationToken);
                    if (read <= 0) throw new EndOfStreamException("Unexpected end of original while applying COPY op.");
                    await output.WriteAsync(buffer, 0, read, cancellationToken);
                    srcPos += read;
                    remaining -= read;
                    newCursor += read;
                }
            }
            else if (op is DiffEngine.DeleteFile delFile)
            {
                if (File.Exists(delFile.FullName))
                {
                    try
                    {
                        File.Delete(delFile.FullName);
                    }
                    catch
                    {
                        // best-effort
                    }

                    string dir = Path.GetDirectoryName(delFile.FullName);
                    if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
                    {
                        try
                        {
                            Directory.Delete(dir);
                        }
                        catch
                        {
                            // best-effort
                        }
                    }
                }
            }
            else if (op is DiffEngine.CreateFile createFile)
            {
                string dir = Path.GetDirectoryName(createFile.FullName);
                if (!Directory.Exists(dir)) 
                    Directory.CreateDirectory(dir);

                await File.WriteAllBytesAsync(createFile.FullName, createFile.Data, cancellationToken);
            }
        }

        await output.FlushAsync(cancellationToken);
    }

    // Attempt to seek the provided stream if possible (best-effort)
    private static void SeekIfPossible(Stream s, long position)
    {
        try
        {
            if (s == null) return;
            if (s.CanSeek)
            {
                s.Seek(position, SeekOrigin.Begin);
            }
        }
        catch { }
    }
}
