using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Serialization.Version2;

/// <summary>
/// Interface that <see cref="ISerializer"/> and <see cref="IDeserializer"/> inherit from. It provides a logger event that can be used to log messages.
/// </summary>
public interface ISerializationWorker
{
    /// <summary>
    /// Logs Syncronously
    /// </summary>
    public event Action<ProgressLog> Logger;
    /// <summary>
    /// Logs Asyncronously
    /// </summary>
    public event Action<ProgressLog> AsyncLogger;
    /// <summary>
    /// Logs Simple progress messages. (Invoked every <see cref="SerializeOptions.SimpleLoggingInterval"/>)"/>
    /// </summary>
    public event Action<ProgressLog> SimpleLogger;

    /// <summary>
    /// The options to configure the serialization or deserialization.
    /// </summary>
    public SerializeOptions Options { get; set; }

    public ParallelLoopState ParallelLoopState { get; }

    public bool Paused { get; }
    public bool Aborted { get; }

    /// <summary>
    /// When overriden in a derived class, Aborts the serialization or deserialization.
    /// </summary>
    public void Abort();

    /// <summary>
    /// When overriden in a derived class, Resumes the serialization or deserialization.
    /// </summary>
    public void Resume();

    /// <summary>
    /// When overriden in a derived class, Pauses the serialization or deserialization.
    /// </summary>
    public void Pause();
}
