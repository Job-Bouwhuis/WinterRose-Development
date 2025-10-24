namespace WinterRose.Recordium;

/// <summary>
/// The severity of a log
/// </summary>
public enum LogSeverity
{
    /// <summary>
    /// The log is for development purposes
    /// </summary>
    Debug,

    /// <summary>
    /// The log is for informational purposes. Mainly used in production so
    /// that information about system state may be tracked in logs
    /// </summary>
    Info,

    /// <summary>
    /// The log represents a problem that the system can recover from without user intervention
    /// or to bring something non critical to the notice of log readers
    /// </summary>
    Warning,

    /// <summary>
    /// The system experienced a failure that may impact app performance or functionality
    /// </summary>
    Error,

    /// <summary>
    /// The system experienced a failure that is sure to impact app performance of functionality,
    /// but the system is most likely to remain usable
    /// </summary>
    Critical,

    /// <summary>
    /// The system experienced a failure that it cant recover from
    /// </summary>
    Fatal
}