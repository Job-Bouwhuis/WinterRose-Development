using System.Runtime.CompilerServices;

namespace WinterRose.Recordium;

public interface ILogger : IDisposable
{
    string Category { get; set; }

    /// <summary>
    /// Writes to all log destinations asynchronously, but does not block the caller to await completion of this write
    /// </summary>
    /// <param name="entry"></param>
    void Write(LogEntry entry);

    LogEntry CreateEntry(LogSeverity severity, string message, string fileName, int lineNumber);
    LogEntry CreateEntry(LogSeverity severity, Exception? ex, string message, string? fileName, int lineNumber);
    Task WriteAsync(LogEntry entry);
    void Debug(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0);
    void Debug(Exception ex, string message = "", [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0);

    Task DebugAsync(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0);

    void Info(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0);

    Task InfoAsync(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0);

    void Warning(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0);
    void Warning(Exception ex, string message = "", [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0);

    Task WarningAsync(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0);

    void Error(string message, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0);
    void Error(Exception ex, string message = "", [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0);

    Task ErrorAsync(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0);

    void Critical(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0);

    void Critical(Exception ex, string message = "", [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0);

    Task CriticalAsync(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0);

    void Fatal(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0);

    void Fatal(Exception ex, string message = "", [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0);

    Task FatalAsync(string message, [CallerFilePath] string? fileName = null,
        [CallerLineNumber] int lineNumber = 0);
}