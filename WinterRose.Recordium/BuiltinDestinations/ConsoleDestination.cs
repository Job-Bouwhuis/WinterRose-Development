namespace WinterRose.Recordium;

public class ConsoleDestination : ILogDestination
{
    public bool Invalidated { get; set; }

    public async Task WriteAsync(LogEntry entry)
    {
        ConsoleColor color = entry.Severity switch
        {
            LogSeverity.Debug => ConsoleColor.Gray,
            LogSeverity.Info => ConsoleColor.White,
            LogSeverity.Warning => ConsoleColor.Yellow,
            LogSeverity.Error => ConsoleColor.Red,
            LogSeverity.Critical => ConsoleColor.Magenta,
            LogSeverity.Catastrophic => ConsoleColor.DarkRed,
            _ => ConsoleColor.White
        };

        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;

        await Console.Out.WriteLineAsync(entry.ToString(LogVerbosity.Detailed));

        Console.ForegroundColor = previousColor;
    }
}