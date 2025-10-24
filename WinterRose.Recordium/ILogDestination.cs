namespace WinterRose.Recordium;

public interface ILogDestination
{
    bool Invalidated { get; set; }
    Task WriteAsync(LogEntry entry);
    /// <summary>
    /// Can be overridden to say submit some logs to the destination that are still buffered.
    /// <br/> Method is called when the app will close either gracefully or as a crash
    /// </summary>
    virtual void Cleanup() {}
}