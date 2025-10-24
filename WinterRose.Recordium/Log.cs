using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WinterRose.Recordium;

/// <summary>
/// A central place to funnel your logs into.
/// </summary>
public class Log
{
    private static readonly List<WeakReference<Log>> LOG_INSTANCES = new();
    private static readonly object LOG_INSTANCES_LOCK = new();

    private static readonly Log UnhandledExceptionsLog = new("Global Unhandled Exceptions");

    public string Category { get; }
    public List<ILogDestination> Destinations { get; }
    private int cleanedUpFlag = 0;

    /// <summary>
    /// Creates a new logger
    /// </summary>
    /// <param name="category">eg "Networking" or "Renderer"</param>
    /// <param name="destinations">Extends on the destinations provided at <see cref="LogDestinations"/></param>
    public Log(string category, params List<ILogDestination> destinations)
    {
        Console.WriteLine("registering log category: " + category);
        Category = category;
        Destinations = LogDestinations.GetAllDestinations(destinations);

        lock (LOG_INSTANCES_LOCK)
        {
            LOG_INSTANCES.Add(new WeakReference<Log>(this));
        }
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

            FlushAll();
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
        {
            FlushAll();
        };
    }
    private void Cleanup()
    {
        if (Interlocked.Exchange(ref cleanedUpFlag, 1) == 1)
            return;

        List<ILogDestination> globalDestinations = LogDestinations.GetAllDestinations();

        foreach (var dest in Destinations)
        {
            if (!globalDestinations.Contains(dest))
                dest.Cleanup();
        }

        // remove this instance from the global list and compact dead refs
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
                {
                    log.Cleanup();
                }
            }
            catch (Exception ex)
            {
                try { Console.WriteLine("Exception while flushing logs: " + ex); } catch { }
            }
        }

        // cleanup dead refs
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