using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeWarden.UserInterface.Content;

/// <summary>
/// Log display content for editor
/// </summary>
public class UILog : UIContent
{
    private readonly List<LogEntry> entries = new();
    private Vector2 logSize = new Vector2(800, 200);
    private const int MAX_LOG_ENTRIES = 100;
    private float scrollOffset = 0f;

    public struct LogEntry
    {
        public string Message;
        public LogLevel Level;
        public DateTime Timestamp;
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public UILog()
    {
    }

    public void AddLogEntry(string message, LogLevel level = LogLevel.Info)
    {
        entries.Add(new LogEntry
        {
            Message = message,
            Level = level,
            Timestamp = DateTime.Now
        });

        // Keep only the last MAX_LOG_ENTRIES
        if (entries.Count > MAX_LOG_ENTRIES)
            entries.RemoveAt(0);

        // Auto-scroll to bottom
        scrollOffset = float.MaxValue;
    }

    public void Clear()
    {
        entries.Clear();
        scrollOffset = 0f;
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        return new Vector2(
            Math.Min(logSize.X, availableArea.Width),
            Math.Min(logSize.Y, availableArea.Height)
        );
    }

    protected override void Draw(Rectangle bounds)
    {
        // Draw background
        Raylib.DrawRectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height, Color.DarkGray);

        // Draw border
        Raylib.DrawRectangleLines((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height, Color.Gray);

        // Create scissor to clip content to bounds
        Raylib.BeginScissorMode((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);

        float currentY = bounds.Y - scrollOffset;
        float lineHeight = 16f;

        foreach (var entry in entries)
        {
            // Calculate color based on level
            Color color = entry.Level switch
            {
                LogLevel.Debug => Color.Gray,
                LogLevel.Info => Color.White,
                LogLevel.Warning => Color.Yellow,
                LogLevel.Error => Color.Red,
                _ => Color.White
            };

            // Draw timestamp and message
            string text = $"[{entry.Timestamp:HH:mm:ss}] {entry.Message}";
            Raylib.DrawText(text, (int)(bounds.X + 4), (int)currentY, 12, color);

            currentY += lineHeight;
        }

        Raylib.EndScissorMode();
    }

    internal protected override float GetHeight(float maxWidth)
    {
        return logSize.Y;
    }
}
