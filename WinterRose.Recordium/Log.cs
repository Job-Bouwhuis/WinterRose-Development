using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WinterRose.Recordium;

/// <summary>
/// A central place to funnel your logs into.
/// </summary>
public class Log
{
    private static readonly Log UnhandledExceptionsLog = new Log("Global Unhandled Exceptions");

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

    private static (string? file, int line) ExtractSource(Exception ex)
    {
        if (ex is AggregateException agg && agg.InnerExceptions.Count > 0)
            ex = agg.InnerExceptions[0];

        var frame = new System.Diagnostics.StackTrace(ex, true).GetFrame(0);
        if (frame != null)
        {
            string? file = frame.GetFileName();
            int line = frame.GetFileLineNumber();
            return (file, line);
        }

        return (null, 0);
    }
    
    static Log()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                var (file, line) = ExtractSource(ex);
                UnhandledExceptionsLog.Catastrophic(ex,
                    $"Unhandled exception thrown!" +
                    $"{(args.IsTerminating ? " This is causing the app to crash!" : "")}",
                    file ?? "Unknown",
                    line);
            }
            else
            {
                UnhandledExceptionsLog.Catastrophic(
                    $"Exception of type {args.ExceptionObject.GetType().Name} thrown and unhandled. " +
                    $"{(args.IsTerminating ? "This is causing the app to crash!" : "")}",
                    "Unknown", 0);
            }
        };
    }

    public void Write(LogEntry entry)
    {
        WriteAsync(entry).GetAwaiter().GetResult();
    }

    private LogEntry CreateEntry(LogSeverity severity, string message, string fileName, int lineNumber)
    {
        return new LogEntry(
            severity,
            Category,
            message,
            fileName,
            lineNumber,
            Environment.CurrentManagedThreadId);
    }
    
    private LogEntry CreateEntry(LogSeverity severity, Exception ex, string message, string fileName, int lineNumber)
    {
        return new LogEntry(
            severity,
            ex,
            Category,
            message,
            fileName,
            lineNumber,
            Environment.CurrentManagedThreadId);
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

    public async Task DebugAsync(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        await WriteAsync(CreateEntry(LogSeverity.Debug, message, fileName, lineNumber));
    }

    public void Info(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        Write(CreateEntry(LogSeverity.Info, message, fileName, lineNumber));
    }

    public async Task InfoAsync(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        await WriteAsync(CreateEntry(LogSeverity.Info, message, fileName, lineNumber));
    }

    public void Warning(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        Write(CreateEntry(LogSeverity.Warning, message, fileName, lineNumber));
    }

    public async Task WarningAsync(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        await WriteAsync(CreateEntry(LogSeverity.Warning, message, fileName, lineNumber));
    }

    public void Error(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        Write(CreateEntry(LogSeverity.Error, message, fileName, lineNumber));
    }

    public void Error(Exception ex, string message = "", [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        Write(CreateEntry(LogSeverity.Error, ex, message, fileName, lineNumber));
    }

    public async Task ErrorAsync(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        await WriteAsync(CreateEntry(LogSeverity.Error, message, fileName, lineNumber));
    }

    public void Critical(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        Write(CreateEntry(LogSeverity.Critical, message, fileName, lineNumber));
    }
    
    public void Critical(Exception ex, string message = "", [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        Write(CreateEntry(LogSeverity.Critical, ex, message, fileName, lineNumber));
    }

    public async Task CriticalAsync(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        await WriteAsync(CreateEntry(LogSeverity.Critical, message, fileName, lineNumber));
    }

    public void Catastrophic(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        Write(CreateEntry(LogSeverity.Catastrophic, message, fileName, lineNumber));
    }
    
    public void Catastrophic(Exception ex, string message = "", [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        Write(CreateEntry(LogSeverity.Catastrophic, ex, message, fileName, lineNumber));
    }

    public async Task CatastrophicAsync(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        await WriteAsync(CreateEntry(LogSeverity.Catastrophic, message, fileName, lineNumber));
    }
}