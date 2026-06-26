using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipelines;

namespace WinterRose.Diff;

public class DirectoryDiffEngine
{
    private readonly DiffEngine diffEngine = new DiffEngine();

    public async Task<DirectoryDiff> DiffAsync(DirectoryInfo oldDir, DirectoryInfo newDir)
    {
        var result = new DirectoryDiff();

        string oldRoot = oldDir.FullName;
        string newRoot = newDir.FullName;

        var oldFiles = oldDir
            .GetFiles("*", SearchOption.AllDirectories)
            .ToDictionary(
                f => GetRelativePath(oldRoot, f.FullName),
                f => f.FullName,
                StringComparer.OrdinalIgnoreCase
            );

        var newFiles = newDir
            .GetFiles("*", SearchOption.AllDirectories)
            .ToDictionary(
                f => GetRelativePath(newRoot, f.FullName),
                f => f.FullName,
                StringComparer.OrdinalIgnoreCase
            );

        var allKeys = new HashSet<string>(oldFiles.Keys, StringComparer.OrdinalIgnoreCase);
        allKeys.UnionWith(newFiles.Keys);

        var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
        var bag = new ConcurrentDictionary<string, FileDiff>(StringComparer.OrdinalIgnoreCase);

        var tasks = allKeys.Select(key =>
            Task.Run(async () =>
            {
                semaphore.Wait();
                try
                {
                    bool oldExists = oldFiles.TryGetValue(key, out var oldPath);
                    bool newExists = newFiles.TryGetValue(key, out var newPath);

                    // =========================
                    // ADDED
                    // =========================
                    if (!oldExists && newExists)
                    {
                        byte[] data = File.ReadAllBytes(newPath!);

                        bag[key] = new FileDiff
                        {
                            State = FileState.Added,
                            ops = new List<DiffEngine.Op>
                            {
                            new DiffEngine.Insert(0, data)
                            }
                        };

                        return;
                    }

                    // =========================
                    // DELETED
                    // =========================
                    if (oldExists && !newExists)
                    {
                        long len = new FileInfo(oldPath!).Length;

                        bag[key] = new FileDiff
                        {
                            State = FileState.Deleted,
                            ops = new List<DiffEngine.Op>
                            {
                            new DiffEngine.Delete(0, len)
                            }
                        };

                        return;
                    }

                    // =========================
                    // MODIFIED / UNCHANGED
                    // =========================
                    if (oldExists && newExists)
                    {
                        var oldFileInfo = new FileInfo(oldPath!);
                        var newFileInfo = new FileInfo(newPath!);

                        if (oldFileInfo.Length == newFileInfo.Length &&
                            await QuickBinaryCompareAsync(oldPath!, newPath!))
                            return; // unchanged

                        var diff = new DiffEngine().Diff(oldPath!, newPath!);

                        if (diff.Count == 0)
                            return; // unchanged

                        bag[key] = new FileDiff
                        {
                            State = FileState.Modified,
                            ops = diff
                        };
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            })
        );

        await Task.WhenAll(tasks);

        result.FileDiffs = new Dictionary<string, FileDiff>(
            bag,
            StringComparer.OrdinalIgnoreCase
        );

        return result;
    }

    // =========================
    // HELPERS
    // =========================

    private static string GetRelativePath(string root, string fullPath)
    {
        return Path.GetRelativePath(root, fullPath)
            .Replace('\\', '/');
    }

    private static async Task<bool> QuickBinaryCompareAsync(string a, string b)
    {
        const int BUFFER_SIZE = 8192;

        await using var fs1 = File.OpenRead(a);
        await using var fs2 = File.OpenRead(b);

        var buffer1 = new byte[BUFFER_SIZE];
        var buffer2 = new byte[BUFFER_SIZE];

        while (true)
        {
            int r1 = await fs1.ReadAsync(buffer1);
            int r2 = await fs2.ReadAsync(buffer2);

            if (r1 != r2)
                return false;

            if (r1 == 0)
                return true;

            for (int i = 0; i < r1; i++)
            {
                if (buffer1[i] != buffer2[i])
                    return false;
            }
        }
    }
}