using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.Recordium;

namespace WinterRoseUtilityApp.SubSystems;

using Raylib_cs;
using System.Collections.Concurrent;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;

internal class InAppLogConsole : ILogDestination
{
    public bool Invalidated { get; set; }

    private readonly List<LogEntry> logEntries = new List<LogEntry>();
    private readonly object logLock = new object();
    UIRows? logRows = null;
    UIWindow? window = null;

    public Task WriteAsync(LogEntry entry)
    {
        lock (logLock)
        {
            logEntries.Add(entry);

            if (logRows != null)
            {
                
                logRows.MoveRows(1);
                logRows.AddToRow(0, FormatLog(entry));
            }
        }

        return Task.CompletedTask;
    }

    public void Show()
    {
        CreateLogWindow().Show();
    }

    private static UIText FormatLog(LogEntry entry)
    {
        string color = entry.Severity switch
        {
            LogSeverity.Debug => "#8A8A8A",
            LogSeverity.Info => "#FFFFFF",
            LogSeverity.Warning => "#FFD166",
            LogSeverity.Error => "#FF6B6B",
            LogSeverity.Critical => "#C77DFF",
            LogSeverity.Fatal => "#FF0000",
            _ => "#FFFFFF"
        };

#if DEBUG
        string text = entry.ToString(LogVerbosity.Detailed);
#else
    string text = entry.ToString();
#endif

        UIText uitext = new UIText($"\\c[{color}]{text}");
        uitext.AutoScaleText = false;
        return uitext;
    }

    private UIWindow CreateLogWindow()
    {
        if (window != null)
            window.Close();

        window = new UIWindow("Application logs", 1500, 900);
        window.OnClosing.Subscribe(Invocation.Create((UIWindow w) =>
        {
            window = null;
            logRows = null;
        }));

        UITextInput searchInput = new UITextInput();
        searchInput.Placeholder = "A ; separated list of search terms";

        window.AddContent(searchInput);

        logRows = new UIRows();
        logRows.RowSpacing = 0;
        logRows.RowPadding = 0;
        logRows.FixedRowHeight = 30;

        List<LogEntry> snapshot;
        lock (logLock)
            snapshot = [.. logEntries];

        for (int i = snapshot.Count-1; i > 0; i--)
        {
            LogEntry entry = snapshot[i];
            logRows.AddToRow(rowIndex: (snapshot.Count-1)-i, content: FormatLog(entry));
            logRows.SetRowAutoSize((snapshot.Count - 1) - i, true);
        }

        window.AddContent(logRows);
        return window;
    }
}

