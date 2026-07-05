using System.Collections.Concurrent;

namespace WinterRose.Diff;

public class FileBackupStore(string key) : IDisposable
{
    private readonly ConcurrentDictionary<string, FileBackup> fileBackups = [];
    private readonly string backupPath = Path.Combine(Path.GetTempPath(), key);
    
    public void Backup(string originalFilePath)
    {
        if (!fileBackups.TryGetValue(originalFilePath, out _))
        {
            FileBackup backup = new(backupPath, originalFilePath);
            backup.EnsureCopy();
            fileBackups.TryAdd(originalFilePath, backup);
        }
    }

    public void Restore(string originalPath)
    {
        if(!fileBackups.TryGetValue(originalPath, out FileBackup fileBackup))
            throw new FileNotFoundException($"File {originalPath} does not exist");
        fileBackup.Restore();
    }

    public void RestoreAll()
    {
        foreach (var fileBackup in fileBackups.Values)
            fileBackup.Restore();
    }

    public void Dispose()
    {
        foreach (var fileBackup in fileBackups.Values)
            fileBackup.Dispose();
    }
}