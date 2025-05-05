using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer;
public class ConsoleLogger : ILogger
{
    private readonly string category;
    private readonly bool enabled;

    public ConsoleLogger(string category, bool enabled = true)
    {
        this.category = category;
        this.enabled = enabled;
    }

    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => enabled;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine($"[{logLevel}] {category}: {formatter(state, exception)}");

        if (exception != null)
        {
            Console.WriteLine(exception);
        }
    }
}
