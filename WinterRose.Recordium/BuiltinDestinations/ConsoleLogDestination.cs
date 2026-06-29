namespace WinterRose.Recordium;

public class ConsoleLogDestination : ILogDestination
{
    public LogVerbosity Verbosity { get; set; }
    public LogSeverity MinumumSeverity { get; set; }

    public ConsoleLogDestination() : this(LogVerbosity.Detailed, LogSeverity.Debug) {}
    
    public ConsoleLogDestination(LogVerbosity verbosity = LogVerbosity.Detailed,
        LogSeverity minumumSeverity = LogSeverity.Debug)
    {
        Verbosity = verbosity;
        MinumumSeverity = minumumSeverity;
    }

    public bool Invalidated { get; set; }

    public async Task WriteAsync(LogEntry entry)
    {
        if (entry.Severity < MinumumSeverity)
            return;

        ConsoleColor color = entry.Severity switch
        {
            LogSeverity.Debug => ConsoleColor.Gray,
            LogSeverity.Info => ConsoleColor.Green,
            LogSeverity.Warning => ConsoleColor.Yellow,
            LogSeverity.Error => ConsoleColor.Red,
            LogSeverity.Critical => ConsoleColor.Magenta,
            LogSeverity.Fatal => ConsoleColor.DarkRed,
            _ => ConsoleColor.White
        };

        var previousForeground = Console.ForegroundColor;
        var previousBackground = Console.BackgroundColor;

        foreach(var part in entry.GetFragments(Verbosity))
        {
            Console.ForegroundColor = part switch
            {
                PrintableLogFragment { Type: LogFragmentType.Timestamp } => ConsoleColor.DarkGray,
                PrintableLogFragment { Type: LogFragmentType.Severity } => color,
                PrintableLogFragment { Type: LogFragmentType.Category } => ConsoleColor.Yellow,
                PrintableLogFragment { Type: LogFragmentType.Message } => ConsoleColor.White,
                PrintableLogFragment { Type: LogFragmentType.Exception } => ConsoleColor.White,
                _ => ConsoleColor.White
            };

            Console.BackgroundColor = part switch
            {
                PrintableLogFragment { Type: LogFragmentType.Message } => 
                    entry.Severity == LogSeverity.Fatal ? ConsoleColor.DarkGray : ConsoleColor.Black,

                _ => ConsoleColor.Black
            };

            await Console.Out.WriteAsync(part.Fragment);
        }

        await Console.Out.WriteLineAsync();

        Console.ForegroundColor = previousForeground;
        Console.BackgroundColor = previousBackground;
    }

    public bool AllowDuplicate(ILogDestination logDestination)
    {
        return false;
    }
}