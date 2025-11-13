using System.Collections.Concurrent;
using System.Text;

namespace WinterRose.Recordium;

public class FileLogDestination : ILogDestination
{
    private Stream fileStream;

    readonly ConcurrentQueue<LogEntry> logEntries = new();

    public void Enqueue(LogEntry entry)
    {
        logEntries.Enqueue(entry);
    }

    public void Cleanup()
    {
        while (logEntries.TryDequeue(out LogEntry entry))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(entry.ToString(LogVerbosity.Detailed) + Environment.NewLine);
            fileStream.Write(bytes, 0, bytes.Length);
        }

        fileStream.Flush();
        fileStream.Dispose();
    }

    public bool TryDequeue(out LogEntry entry)
    {
        if (logEntries.TryDequeue(out entry))
            return true;

        entry = null;
        return false;
    }

    public FileLogDestination(string fileDirectory)
    {
        string date = DateTime.UtcNow.ToString("yyyy.MM.dd_HH.mm.ss");

        DirectoryInfo di = new DirectoryInfo(fileDirectory);
        if (!di.Exists)
            di.Create();

        CleanupOldFiles(di);

        string fileName = Path.Combine(di.FullName, date + ".log");

        FileInfo fi = new FileInfo(fileName);
        fileStream = new FileStream(fileName, FileMode.Create);

        FileInfo shortcut = new FileInfo(Path.Combine(di.FullName, "latest.log"));
        if (shortcut.Exists)
            shortcut.Delete();
        shortcut.CreateAsSymbolicLink(fi.FullName);
    }

    public bool Invalidated { get; set; }

    public async Task WriteAsync(LogEntry entry)
    {
        Enqueue(entry);
        if (logEntries.Count > CommitEvery)
            await CommitWrite();
    }

    private async Task CommitWrite()
    {
        while (TryDequeue(out LogEntry entry))
        {
            await fileStream.WriteAsync(
                Encoding.UTF8.GetBytes(entry.ToString(LogVerbosity.Detailed) + Environment.NewLine));
            await fileStream.FlushAsync();
        }
    }

    private void CleanupOldFiles(DirectoryInfo dir)
    {
        var files = dir.GetFiles("*.log")
            .OrderBy(f => f.CreationTime)
            .ToList();

        while (files.Count > MaxFilesToKeep)
        {
            files[0].Delete();
            files.RemoveAt(0);
        }
    }

    private const int MaxFilesToKeep
        = 10;

    private const int CommitEvery = 50;
}