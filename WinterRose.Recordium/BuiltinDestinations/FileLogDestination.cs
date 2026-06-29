using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using WinterRose.FileManagement.Shortcuts;

namespace WinterRose.Recordium;

public class FileLogDestination : ILogDestination
{
    public string FileDirectory { get; set; }
    public LogVerbosity Verbosity { get; set; }
    public LogSeverity MinumumSeverity { get; set; }
    
    private FileStream? fileStream;
    readonly ConcurrentQueue<LogEntry> logEntries = new();

    public bool Invalidated { get; set; }

    public FileLogDestination() : this("logs", LogVerbosity.Detailed, LogSeverity.Debug)
    {
        
    }
    
    public FileLogDestination(string fileDirectory = "logs", 
        LogVerbosity verbosity = LogVerbosity.Detailed,
        LogSeverity minumumSeverity = LogSeverity.Debug)
    {
        FileDirectory = fileDirectory;
        Verbosity = verbosity;
        MinumumSeverity = minumumSeverity;
    }

    private void Setup()
    {
        if (fileStream != null)
            return;
        string date = DateTime.UtcNow.ToString("yyyy.MM.dd_HH.mm.ss");

        DirectoryInfo di = new DirectoryInfo(FileDirectory);
        if (!di.Exists)
            di.Create();

        CleanupOldFiles(di);

        string fileName = Path.Combine(di.FullName, date + ".log");

        FileInfo fi = new FileInfo(fileName);
        fileStream = new FileStream(fileName, FileMode.Create);

        ShortcutMaker.CreateShortcut(
            shortcutPath: Path.Combine(di.FullName, "Latest Log"),
            targetPath: fi.FullName
        );
    }

    public void Enqueue(LogEntry entry)
    {
        Setup();
        logEntries.Enqueue(entry);
    }

    public void Cleanup()
    {
        while (logEntries.TryDequeue(out LogEntry entry))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(entry.ToString(LogVerbosity.Detailed) + Environment.NewLine);
            fileStream.Write(bytes, 0, bytes.Length);
        }

        try
        {
            fileStream.Flush();
            fileStream.Dispose();
        }
        catch
        {
            // Ignore exceptions during cleanup
        }
    }

    public bool AllowDuplicate(ILogDestination logDestination)
    {
        if (logDestination is FileLogDestination f)
            return f.fileStream.Name != fileStream.Name;
        throw new ArgumentException("logDestination must be a FileLogDestination",  nameof(logDestination));
    }

    public bool TryDequeue([NotNullWhen(true)] out LogEntry? entry)
    {
        if (logEntries.TryDequeue(out entry))
            return true;

        return false;
    }
    
    public async Task WriteAsync(LogEntry entry)
    {
        if(entry.Severity < MinumumSeverity)
            return;
        Enqueue(entry);
        if (logEntries.Count > CommitEvery)
            await CommitWrite();
    }

    private async Task CommitWrite()
    {
        while (TryDequeue(out LogEntry entry))
        {
            await fileStream.WriteAsync(
                Encoding.UTF8.GetBytes(entry.ToString(Verbosity) + Environment.NewLine));
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