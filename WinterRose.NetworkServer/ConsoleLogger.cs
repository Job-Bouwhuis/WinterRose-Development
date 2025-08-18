using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer;
public class ConsoleLogger : ILogger
{
    public string Category { get; set; }
    public bool Enabled { get; set; }

    public ConsoleLogger(string category, bool enabled = true)
    {
        this.Category = category;
        this.Enabled = enabled;
    }

    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => Enabled;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!Enabled) return;

        // Save current console colors
        var originalForeground = Console.ForegroundColor;
        var originalBackground = Console.BackgroundColor;

        try
        {
            // Set color based on log level
            Console.ForegroundColor = GetColorForLevel(logLevel);

            // Timestamp
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

            // Log line prefix: [time] [LEVEL] Category:
            Console.Write($"[{timestamp}] [{logLevel.ToString().ToUpper()}] ");

            // Category in bold-ish style (bright white)
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{Category}: ");

            // Message
            Console.ForegroundColor = GetColorForLevel(logLevel);
            Console.WriteLine(formatter(state, exception));

            // If there's an exception, print it indented and in red
            if (exception != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                PrintException(exception, indentLevel: 1);
            }
        }
        finally
        {
            // Restore original colors
            Console.ForegroundColor = originalForeground;
            Console.BackgroundColor = originalBackground;
        }
    }

    private static ConsoleColor GetColorForLevel(LogLevel level) => level switch
    {
        LogLevel.Trace => ConsoleColor.DarkGray,
        LogLevel.Debug => ConsoleColor.Gray,
        LogLevel.Information => ConsoleColor.Green,
        LogLevel.Warning => ConsoleColor.Yellow,
        LogLevel.Error => ConsoleColor.Red,
        LogLevel.Critical => ConsoleColor.Magenta,
        _ => ConsoleColor.White,
    };

    private void PrintException(Exception exception, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 4);
        Console.WriteLine($"{indent}Exception: {exception.Message}");

        if (exception.StackTrace != null)
        {
            var stackLines = exception.StackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in stackLines)
            {
                Console.WriteLine($"{indent}    {line}");
            }
        }

        if (exception.InnerException != null)
        {
            Console.WriteLine($"{indent}Inner Exception:");
            PrintException(exception.InnerException, indentLevel + 1);
        }
    }
}

