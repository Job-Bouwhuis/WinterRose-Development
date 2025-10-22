namespace WinterRose.Recordium;

public interface ILogDestination
{
    bool Invalidated { get; set; }
    Task WriteAsync(LogEntry entry);
}