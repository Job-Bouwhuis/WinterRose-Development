using System.Diagnostics;

namespace WinterRose.Recordium;

public enum LogVerbosity
{
    Minimal,
    Normal,
    Detailed,
    Full
}

/// <summary>
/// A log entry in the Recordium logging system
/// </summary>
[DebuggerDisplay("{DebuggerDisplay}")]
public class LogEntry
{
    private string DebuggerDisplay => ToString(LogVerbosity.Minimal);

    /// <summary>
    /// The severity of the log
    /// </summary>
    public LogSeverity Severity { get; set; }
    /// <summary>
    /// The log message
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// When the log was submitted
    /// </summary>
    public DateTime Timestamp { get; set; }
    /// <summary>
    /// The category for the log. eg "Renderer" or "IO"
    /// </summary>
    public string Category { get; set; }
    /// <summary>
    /// The exception that was thrown (may be null if the log does not concern an exception)
    /// </summary>
    public Exception? Exception { get; set; }
    /// <summary>
    /// The stack trace of the exception thrown (null if <see cref="LogEntry.Exception"/> is also null)
    /// </summary>
    public StackTrace? StackTrace { get; set; }
    /// <summary>
    /// The file name where the log originates from
    /// </summary>
    public string FileName { get; set; }
    /// <summary>
    /// The line number within the file where the log originates from
    /// </summary>
    public int LineNumber { get; set; }
    /// <summary>
    /// The threadID of the thread that submitted the log
    /// </summary>
    public int ThreadId { get; set; }

    /// <summary>
    /// Creates a log entry
    /// </summary>
    public LogEntry(LogSeverity severity, string category, string message, string fileName, int lineNumber, int threadId)
    {
        Severity = severity;
        Message = message;
        Category = category;
        FileName = fileName;
        LineNumber = lineNumber;
        ThreadId = threadId;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a log entry with an exception
    /// </summary>
    public LogEntry(
        LogSeverity severity,
        Exception exception,
        string category,
        string? message = null,
        string? fileName = null,
        int lineNumber = 0,
        int threadId = 0)
    {
        Severity = severity;
        Exception = exception;
        Message = message ?? "";
        Category = category;
        FileName = fileName ?? "Unknown";
        LineNumber = lineNumber;
        ThreadId = threadId;
        Timestamp = DateTime.UtcNow;
        StackTrace = exception != null ? new StackTrace(exception, true) : null;
    }
    
    public override string ToString() => ToString(LogVerbosity.Normal);

    public string ToString(LogVerbosity verbosity)
    {
        string shortFile = Path.GetFileName(FileName);
        string time = Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

        var res = verbosity switch
        {
            LogVerbosity.Minimal =>
                $"{Severity}: {Message}",

            LogVerbosity.Normal =>
                $"[{time}] [{Severity}] {Message}",

            LogVerbosity.Detailed =>
                $"[{time}] [{Category}] {shortFile}:{LineNumber} - [{Severity}]: {Message}",

            LogVerbosity.Full =>
                BuildFullString(time, shortFile),

            _ => ToString()
        };

        if(verbosity is not LogVerbosity.Full && Exception != null)
                res += FormatException(Exception, 1);
        return res;
    }

    private string BuildFullString(string time, string shortFile)
    {
        string info = $"[{time}] [{Severity}] ({Category}) {Message} " +
                      $"({shortFile}:{LineNumber}, Thread {ThreadId})";

        if (Exception != null)
            info += FormatException(Exception, 1);

        return info;
    }

    private static string FormatException(Exception ex, int indentLevel = 0)
    {
        string indent = new string('\t', indentLevel);
        string result = $"{Environment.NewLine}{indent}Exception: {ex.GetType().FullName}: {ex.Message}";
        result += $"{Environment.NewLine}{indent}{ex.StackTrace}";

        if (ex.InnerException != null)
            result += $"{Environment.NewLine}{indent}Inner Exception →" +
                      FormatException(ex.InnerException, indentLevel + 1);

        return result;
    }

}
