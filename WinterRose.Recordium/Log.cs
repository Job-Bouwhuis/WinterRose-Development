using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WinterRose.Recordium;

/// <summary>
/// A central place to funnel your logs into.
/// </summary>
public class Log : ILogger
{
    private static readonly List<WeakReference<Log>> LOG_INSTANCES = new();
    private static readonly object LOG_INSTANCES_LOCK = new();

    private static readonly Log UnhandledExceptionsLogger = new("Global Unhandled Exceptions");
    private static readonly Dictionary<LogVerbosity, string> DEFAULT_TEMPLATES = new()
    {
        [LogVerbosity.Minimal] = "[{severity}] {message} {exception}",
        [LogVerbosity.Normal] = "[{time}] [{severity}] [{category}] {message} {exception}",
        [LogVerbosity.Detailed] = "[{time}] [{severity}] [{category}] {message} {?file:(in {file})|} {exception}",
        [LogVerbosity.Full] = "[{time}] [{severity}] [{category}] {message} {?file:(in {file})|} {?thread:in thread {thread}|} {exception}"
    };

    private static readonly Dictionary<LogVerbosity, string?> CUSTOM_TEMPLATES = new();

    public static string GetTemplate(LogVerbosity verbosity)
    {
        if (CUSTOM_TEMPLATES.TryGetValue(verbosity, out var custom) &&
            !string.IsNullOrWhiteSpace(custom))
        {
            return custom;
        }

        return DEFAULT_TEMPLATES[verbosity];
    }

    public static void SetTemplate(LogVerbosity verbosity, string template)
    {
        CUSTOM_TEMPLATES[verbosity] = template;
    }

    public static void ResetTemplate(LogVerbosity verbosity)
    {
        CUSTOM_TEMPLATES.Remove(verbosity);
    }


    public string Category { get; set; }
    public IReadOnlyList<ILogDestination> Destinations { get; }
    private int cleanedUpFlag = 0;

    public Log(string category)
    {
        Category = category;
        Destinations = LogDestinations.GetAllDestinations();

        lock (LOG_INSTANCES_LOCK)
            LOG_INSTANCES.Add(new WeakReference<Log>(this));
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
                UnhandledExceptionsLogger.Fatal(ex,
                    $"Unhandled exception thrown!" +
                    $"{(args.IsTerminating ? " This is causing the app to crash!" : "")}",
                    file ?? "Unknown",
                    line);
            }
            else
            {
                UnhandledExceptionsLogger.Fatal(
                    $"Exception of type {args.ExceptionObject.GetType().Name} thrown and unhandled. " +
                    $"{(args.IsTerminating ? "This is causing the app to crash!" : "")}",
                    "Unknown", 0);
            }

            FlushAll();
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
        {
            FlushAll();
        };
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref cleanedUpFlag, 1) == 1)
            return;

        IReadOnlyList<ILogDestination> globalDestinations = LogDestinations.GetAllDestinations();

        foreach (var dest in Destinations)
        {
            if (!globalDestinations.Contains(dest))
                dest.Cleanup();
        }

        lock (LOG_INSTANCES_LOCK)
        {
            for (int i = LOG_INSTANCES.Count - 1; i >= 0; i--)
            {
                var wr = LOG_INSTANCES[i];
                if (wr.TryGetTarget(out var target))
                {
                    if (ReferenceEquals(target, this))
                        LOG_INSTANCES.RemoveAt(i);
                }
                else
                {
                    LOG_INSTANCES.RemoveAt(i);
                }
            }
        }
    }
    
    private static void FlushAll()
    {
        List<WeakReference<Log>> snapshot;
        lock (LOG_INSTANCES_LOCK)
        {
            snapshot = LOG_INSTANCES.ToList();
        }

        foreach (var weak in snapshot)
        {
            try
            {
                if (weak.TryGetTarget(out var log))
                    log.Dispose();
            }
            catch // we dont care about these errors
            {
            }
        }
        
        lock (LOG_INSTANCES_LOCK)
        {
            for (int i = LOG_INSTANCES.Count - 1; i >= 0; i--)
            {
                if (!LOG_INSTANCES[i].TryGetTarget(out _))
                    LOG_INSTANCES.RemoveAt(i);
            }
        }

        foreach (var globalDest in LogDestinations.GetAllDestinations())
        {
            globalDest.Cleanup();
        }
    }

    /// <summary>
    /// Writes to all log destinations asynchronously, but does not block the caller to await completion of this write
    /// </summary>
    /// <param name="entry"></param>
    public void Write(LogEntry entry)
    {
        _ = WriteAsync(entry);
    }

    public LogEntry CreateEntry(LogSeverity severity, string message, string fileName, int lineNumber)
    {
        return new LogEntry(
            severity,
            Category,
            message,
            fileName,
            lineNumber,
            Environment.CurrentManagedThreadId);
    }

    public LogEntry CreateEntry(LogSeverity severity, Exception? ex, string message, string? fileName, int lineNumber)
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

    public void Debug(Exception ex, string message = "", [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        Write(CreateEntry(LogSeverity.Debug, ex, message, fileName, lineNumber));
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

    public void Warning(Exception ex, string message = "", [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        Write(CreateEntry(LogSeverity.Warning, ex, message, fileName, lineNumber));
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

    public void Fatal(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        Write(CreateEntry(LogSeverity.Fatal, message, fileName, lineNumber));
    }
    
    public void Fatal(Exception ex, string message = "", [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        Write(CreateEntry(LogSeverity.Fatal, ex, message, fileName, lineNumber));
    }

    public async Task FatalAsync(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        await WriteAsync(CreateEntry(LogSeverity.Fatal, message, fileName, lineNumber));
    }

    public static void Flush()
    {
        FlushAll();
    }

    public static Log? GetLogger(string category)
    {
        lock (LOG_INSTANCES_LOCK)
        {
            foreach (var weak in LOG_INSTANCES)
            {
                if (weak.TryGetTarget(out var log))
                {
                    if (log.Category == category)
                        return log;
                }
            }
        }
        return new Log(category);
    }
}