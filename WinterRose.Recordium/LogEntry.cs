namespace WinterRose.Recordium;

public enum LogVerbosity
{
    Minimal,
    Normal,
    Detailed,
    Full
}

public class LogEntry
{
    public LogSeverity Severity { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public string Category { get; set; }
    public Exception? Exception { get; set; }
    public string FileName { get; set; }
    public int LineNumber { get; set; }
    public int ThreadId { get; set; }

    public override string ToString()
    {
        string shortFile = Path.GetFileName(FileName);
        string time = Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

        return $"[{time}] [{Category}] {Severity}: {Message} ({shortFile}:{LineNumber}, T{ThreadId})";
    }

    public string ToString(LogVerbosity verbosity)
    {
        string shortFile = Path.GetFileName(FileName);
        string time = Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

        return verbosity switch
        {
            LogVerbosity.Minimal =>
                $"{Severity}: {Message}",

            LogVerbosity.Normal =>
                $"[{time}] [{Severity}] {Message}",

            LogVerbosity.Detailed =>
                $"[{time}] [{Category}] {Severity}: {Message} ({shortFile}:{LineNumber})",

            LogVerbosity.Full =>
                BuildFullString(time, shortFile),

            _ => ToString()
        };
    }

    private string BuildFullString(string time, string shortFile)
    {
        string info = $"[{time}] [{Severity}] ({Category}) {Message} " +
                      $"({shortFile}:{LineNumber}, Thread {ThreadId})";

        if (Exception != null)
            info += $"{Environment.NewLine}Exception: {Exception}";

        return info;
    }
}
