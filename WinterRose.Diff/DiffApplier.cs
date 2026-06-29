using WinterRose.ProgressKeeping;

namespace WinterRose.Diff;

public class DiffApplier
{
    public bool ApplyDiff(string filePath, FileDiff diff, IProgressScope? progress = null)
    {
        try
        {
            if (diff.State == FileState.Added)
            {
                File.Create(filePath);
                diff.State = FileState.Modified;
            }

            if (diff.State == FileState.Deleted)
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);

                progress?.Report(1.0, $"Deleted {Path.GetFileName(filePath)}");
                return true;
            }

            if (diff.State == FileState.Modified)
            {
                using FileStream file = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

                long delta = 0;
                int totalOps = diff.Operations.Count;
                int completedOps = 0;

                foreach (var op in diff.Operations)
                {
                    if (op is Insert insert)
                    {
                        var shifted = new Insert(insert.Offset + delta, insert.Data);
                        ApplyInsert(file, shifted);
                        delta += insert.Data.Length;
                    }
                    else if (op is Delete delete)
                    {
                        var shifted = new Delete(delete.Offset + delta, delete.Length);
                        ApplyDelete(file, shifted);
                        delta -= delete.Length;
                    }
                    else if (op is Update update)
                    {
                        long sizeDelta = ApplyUpdate(file, delta, update);
                        delta += sizeDelta;
                    }
                    else if (op is DeleteFile)
                    {
                        file.Close();
                        File.Delete(filePath);
                    }

                    completedOps++;

                    if (progress != null && totalOps > 0)
                    {
                        double frac = (double)completedOps / totalOps;
                        string opLabel = op switch
                        {
                            Insert  => "insert",
                            Delete  => "delete",
                            Update  => "update",
                            DeleteFile => "delete file",
                            _ => "op"
                        };
                        progress.Report(frac, $"{Path.GetFileName(filePath)}: {opLabel} ({completedOps}/{totalOps})");
                    }
                }

                // If there were no operations, still mark complete
                if (totalOps == 0)
                    progress?.Report(1.0, Path.GetFileName(filePath));
            }

            return true;
        }
        catch (Exception)
        {
            progress?.Report(1.0, $"Failed: {Path.GetFileName(filePath)}");
            return false;
        }
    }

    public async Task<List<string>> ApplyDiff(string targetDirectory, DirectoryDiff diff, IProgressScope? progress = null)
    {
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
        var failedFiles = new System.Collections.Concurrent.ConcurrentBag<string>();

        // Weight each file's child scope by its operation count so overall
        // progress reflects actual work rather than raw file count.
        var tasks = diff.FileDiffs.Select(kvp =>
        {
            string relativePath = kvp.Key;
            FileDiff fileDiff = kvp.Value;

            // Create the child scope before entering the task so weights are
            // registered on the parent before any work starts.
            double weight = Math.Max(1, fileDiff.Operations.Count);
            IProgressScope? fileScope = progress?.CreateChild(weight);

            return Task.Run(async () =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);

                try
                {
                    string targetPath = Path.Combine(targetDirectory, relativePath);
                    string? directory = Path.GetDirectoryName(targetPath);

                    if (!string.IsNullOrEmpty(directory))
                        Directory.CreateDirectory(directory);

                    bool applied = ApplyDiff(targetPath, fileDiff, fileScope);
                    if (!applied)
                    {
                        failedFiles.Add(relativePath);
                        return;
                    }

                    if (!string.IsNullOrEmpty(fileDiff.NewFileHash))
                    {
                        fileScope?.Report(1.0, $"Verifying {relativePath}...");

                        using FileView view = new(targetPath);
                        string actualHash = view.ComputeSha256();

                        if (!string.Equals(actualHash, fileDiff.NewFileHash, StringComparison.OrdinalIgnoreCase))
                        {
                            failedFiles.Add(relativePath);
                            fileScope?.Report(1.0, $"{relativePath} failed. Queued for redownload");
                        }
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
        return failedFiles.ToList();
    }

    private long ApplyUpdate(FileStream file, long delta, Update update)
    {
        long liveOffset = update.Offset + delta;
        long sizeDelta = update.Data.Length - update.Length;

        if (sizeDelta == 0)
        {
            // Perfect replacement — pure overwrite, zero shifting
            file.Position = liveOffset;
            file.Write(update.Data);
        }
        else if (sizeDelta < 0)
        {
            // New data is shorter — write new data, then collapse the gap
            file.Position = liveOffset;
            file.Write(update.Data);

            // Delete only the leftover gap after the new data
            var gap = new Delete(liveOffset + update.Data.Length, -sizeDelta);
            ApplyDelete(file, gap);
        }
        else
        {
            // New data is longer — make room for only the overflow, then write
            var overflow = new Insert(liveOffset + update.Length, new byte[sizeDelta]);
            ApplyInsert(file, overflow);

            file.Position = liveOffset;
            file.Write(update.Data);
        }

        return sizeDelta;
    }

    private void ApplyInsert(FileStream file, Insert insert)
    {
        long offset = insert.Offset;
        byte[] data = insert.Data;

        file.Seek(0, SeekOrigin.End);
        long end = file.Position;

        long moveSize = end - offset;

        byte[] buffer = new byte[8192];

        long readPos = end;
        long writePos = end + data.Length;

        while (moveSize > 0)
        {
            int chunkSize = (int)Math.Min(buffer.Length, moveSize);

            readPos -= chunkSize;
            file.Position = readPos;
            file.ReadExactly(buffer.AsSpan(0, chunkSize));

            writePos -= chunkSize;
            file.Position = writePos;
            file.Write(buffer.AsSpan(0, chunkSize));

            moveSize -= chunkSize;
        }

        file.Position = offset;
        file.Write(data);
    }

    private void ApplyDelete(FileStream file, Delete delete)
    {
        long offset = delete.Offset;
        long length = delete.Length;

        file.Seek(0, SeekOrigin.End);
        long end = file.Position;

        long readPos = offset + length;
        long writePos = offset;

        byte[] buffer = new byte[8192];

        long remaining = end - readPos;

        while (remaining > 0)
        {
            int chunkSize = (int)Math.Min(buffer.Length, remaining);

            file.Position = readPos;
            file.ReadExactly(buffer.AsSpan(0, chunkSize));

            file.Position = writePos;
            file.Write(buffer.AsSpan(0, chunkSize));

            readPos += chunkSize;
            writePos += chunkSize;
            remaining -= chunkSize;
        }

        file.SetLength(end - length);
    }

}