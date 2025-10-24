using System.Text;

namespace WinterRose.Recordium;

public class FileDestination : ILogDestination
{
    private int prioNum = 0;
    private Stream fileStream;

    private Lock locker = new();
    readonly PriorityQueue<LogEntry, int> logEntries = new();

    public void Enqueue(LogEntry entry)
    {
        locker.Enter();
        logEntries.Enqueue(entry, prioNum++);
        locker.Exit();
    }

    public void Cleanup()
    {
        locker.Enter();
        try
        {
            while (logEntries.TryDequeue(out LogEntry entry, out _))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(entry.ToString(LogVerbosity.Detailed) + Environment.NewLine);
                fileStream.Write(bytes, 0, bytes.Length);
            }

            fileStream.Flush();
            fileStream.Dispose();
        }
        finally
        {
            locker.Exit();
        }
    }

    public bool TryDequeue(out LogEntry entry)
    {
        locker.Enter();
        try
        {
            if (logEntries.TryDequeue(out entry, out _))
                return true;

            entry = null;
            return false;
        }
        finally
        {
            locker.Exit();
        }
    }

    public FileDestination(string fileDirectory)
    {
        string date = DateTime.UtcNow.ToString("yyyy.MM.dd_HH.mm.ss");

        DirectoryInfo di = new DirectoryInfo(fileDirectory);
        if (!di.Exists)
            di.Create();

        CleanupOldFiles(di);

        string fileName = Path.Combine(di.FullName, date + ".log");

        FileInfo fi = new FileInfo(fileName);
        fileStream = new FileStream(fileName, FileMode.Create);

        // create shortcut in same dir to latest file (the one were creating now)
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
        locker.Enter();
        while (TryDequeue(out LogEntry entry))
        {
            await fileStream.WriteAsync(
                Encoding.UTF8.GetBytes(entry.ToString(LogVerbosity.Detailed) + Environment.NewLine));
            await fileStream.FlushAsync();
        }

        locker.Exit();
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