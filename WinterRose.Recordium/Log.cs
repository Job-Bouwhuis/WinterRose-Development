using System.Runtime.CompilerServices;

namespace WinterRose.Recordium;

/// <summary>
/// A central place to funnel your logs into.
/// </summary>
public class Log
{
    public string Category { get; }
    public List<ILogDestination> Destinations { get; }

    /// <summary>
    /// Creates a new logger
    /// </summary>
    /// <param name="category">eg "Networking" or "Renderer"</param>
    /// <param name="destinations">Extends on the destinations provided at <see cref="LogDestinations"/></param>
    public Log(string category, params List<ILogDestination> destinations)
    {
        Category = category;
        Destinations = LogDestinations.GetAllDestinations(destinations);
    }

    public void Write(LogEntry entry)
    {
        WriteAsync(entry).GetAwaiter().GetResult();

    }

    private LogEntry CreateEntry(LogSeverity severity, string message, string? fileName, int lineNumber)
    {
        return new LogEntry
        {
            Category = Category,
            Message = message,
            FileName = fileName,
            LineNumber = lineNumber,
            Severity = severity,
            Timestamp = DateTime.UtcNow,
            ThreadId = Thread.CurrentThread.ManagedThreadId
        };
    }


    public async Task WriteAsync(LogEntry entry)
    {
        List<Task> writeTasks = [];
        foreach (var destination in Destinations)
            writeTasks.Add(destination.WriteAsync(entry));
        await Task.WhenAll(writeTasks);
    }

   public void Debug(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
{
    Write(CreateEntry(LogSeverity.Debug, message, fileName, lineNumber));
}

public async Task DebugAsync(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
{
    await WriteAsync(CreateEntry(LogSeverity.Debug, message, fileName, lineNumber));
}

public void Info(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
{
    Write(CreateEntry(LogSeverity.Info, message, fileName, lineNumber));
}

public async Task InfoAsync(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
{
    await WriteAsync(CreateEntry(LogSeverity.Info, message, fileName, lineNumber));
}

public void Warning(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
{
    Write(CreateEntry(LogSeverity.Warning, message, fileName, lineNumber));
}

public async Task WarningAsync(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
{
    await WriteAsync(CreateEntry(LogSeverity.Warning, message, fileName, lineNumber));
}

public void Error(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
{
    Write(CreateEntry(LogSeverity.Error, message, fileName, lineNumber));
}

public async Task ErrorAsync(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
{
    await WriteAsync(CreateEntry(LogSeverity.Error, message, fileName, lineNumber));
}

public void Critical(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
{
    Write(CreateEntry(LogSeverity.Critical, message, fileName, lineNumber));
}

public async Task CriticalAsync(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
{
    await WriteAsync(CreateEntry(LogSeverity.Critical, message, fileName, lineNumber));
}

public void Catastrophic(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
{
    Write(CreateEntry(LogSeverity.Catastrophic, message, fileName, lineNumber));
}

public async Task CatastrophicAsync(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
{
    await WriteAsync(CreateEntry(LogSeverity.Catastrophic, message, fileName, lineNumber));
}

}