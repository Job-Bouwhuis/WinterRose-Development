using System.Diagnostics.Tracing;
using System.Text.RegularExpressions;
using WinterRose.ProgressKeeping;

namespace WinterRose.Diff;

public class DiffApplier
{
    public async Task ApplyDiff(string filePath, FileDiff diff, IProgressScope? progress = null)
    {
        progress ??= new ProgressScope();
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

                await progress.ReportAsync(1.0, $"Deleted {Path.GetFileName(filePath)}", ReportStatus.Info);
                return;
            }

            if (diff.State == FileState.Modified)
            {
                using FileStream file = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                    FileShare.None);

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
                            Insert => "insert",
                            Delete => "delete",
                            Update => "update",
                            DeleteFile => "delete file",
                            _ => "op"
                        };
                        await progress.ReportAsync(frac, $"{Path.GetFileName(filePath)}: {opLabel} ({completedOps}/{totalOps})", ReportStatus.Info);
                    }
                }

                if (totalOps == 0)
                    await progress.ReportAsync(1.0, Path.GetFileName(filePath), ReportStatus.Info);
            }

            return;
        }
        catch (Exception)
        {
            await progress.ReportAsync(1.0, $"Failed: {Path.GetFileName(filePath)}", ReportStatus.Error);
        }
    }

    public async Task ApplyDiff(
        string targetDirectory, 
        DirectoryDiff diff,
        IProgressScope progress,
        Func<string, Task<AlternativeFile>>? fileReplacementGetter = null)
    {
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
        var failedFiles = new System.Collections.Concurrent.ConcurrentBag<string>();
        using var backupStore = new FileBackupStore(FileBackup.GeneratePathHash(targetDirectory));
        
        // Weight each file's child scope by its operation count so overall
        // progress reflects actual work rather than raw file count.
        var tasks = diff.FileDiffs.Select(kvp =>
        {
            string relativePath = kvp.Key;
            FileDiff fileDiff = kvp.Value;

            // Create the child scope before entering the task so weights are
            // registered on the parent before any work starts.
            double weight = Math.Max(1, fileDiff.Operations.Count);
            IProgressScope fileScope = progress.CreateChild(weight);

            return Task.Run(async () =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);

                try
                {
                    string targetPath = Path.Combine(targetDirectory, relativePath);
                    string? directory = Path.GetDirectoryName(targetPath);
                    
                    backupStore.Backup(targetPath);
                    
                    if (!string.IsNullOrEmpty(directory))
                        Directory.CreateDirectory(directory);

                    await ApplyDiff(targetPath, fileDiff, fileScope);

                    if (!string.IsNullOrEmpty(fileDiff.NewFileHash))
                    {
                        await fileScope.ReportAsync(0.9, $"Verifying {relativePath}...", ReportStatus.Info);

                        FileView view = new(targetPath);
                        string actualHash = view.ComputeSha256();
                        view.Dispose();
                        
                        if (!string.Equals(actualHash, fileDiff.NewFileHash, StringComparison.OrdinalIgnoreCase))
                        {
                            if (fileReplacementGetter is not null)
                            {
                                int retries = 0;
                                int maxRetries = 5;
                                while (retries++ < maxRetries)
                                {
                                    await fileScope.ReportAsync(0.95, $"{relativePath} failed. Fetching replacement...", ReportStatus.Warning);
                                    await using var replacement = await fileReplacementGetter(relativePath);
                                
                                    await fileScope.ReportAsync(0.95, $"Applying replacement...", ReportStatus.Warning);
                                    await using FileStream f = File.Open(targetPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                                    f.Position = 0;
                                    f.SetLength(0);
                                    await replacement.CopyToAsync(f);

                                    using FileView v = new FileView(f);
                                    await fileScope.ReportAsync(0.95, $"verifying replacement...", ReportStatus.Warning);
                                    string hash = v.ComputeSha256();
                                    if (hash != replacement.Hash)
                                    {
                                        await fileScope.ReportAsync(0.95, $"Failed to apply replacement. Retrying ({retries}/{maxRetries})", ReportStatus.Error);
                                        continue;
                                    }
                                    
                                    await fileScope.ReportAsync(1, $"Replacement successfully applied.", ReportStatus.Success);
                                    break;
                                }
                            }
                            else
                            {
                                await fileScope.ReportAsync(1.0, $"{relativePath} failed", ReportStatus.Error);
                                failedFiles.Add(relativePath);
                            }
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

        if (failedFiles.Count > 0)
        {
            await progress.ReportAsync(0.95, "Some files failed to apply or get a replacement. Restoring backups...", ReportStatus.Error);
            backupStore.RestoreAll();
            await progress.ReportAsync(1, "Backups restored.", ReportStatus.Info);
        }
        else
            await progress.ReportAsync(1, "Patch complete.", ReportStatus.Success);
    }


    private long ApplyUpdate(FileStream file, long delta, Update update)
    {
        long liveOffset = update.Offset + delta;
        long sizeDelta = update.Data.Length - update.Length;

        if (sizeDelta == 0)
        {
            // Perfect replacement, pure overwrite, zero shifting
            file.Position = liveOffset;
            file.Write(update.Data);
        }
        else if (sizeDelta < 0)
        {
            // New data is shorter, write new data, then collapse the gap
            file.Position = liveOffset;
            file.Write(update.Data);

            // Delete only the leftover gap after the new data
            var gap = new Delete(liveOffset + update.Data.Length, -sizeDelta);
            ApplyDelete(file, gap);
        }
        else
        {
            // New data is longer, make room for only the overflow, then write
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