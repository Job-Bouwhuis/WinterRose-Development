using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Serialization.Version2
{
    /// <summary>
    /// Logged progress of a serialization or deserialization.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of the <see cref="ProgressLog"/> struct.
    /// </remarks>
    /// <param name="thread"></param>
    /// <param name="progress"></param>
    /// <param name="total"></param>
    /// <param name="message"></param>
    public readonly struct ProgressLog(int thread, int handledThisCycle, int progress, int total, string message)
    {
        /// <summary>
        /// The thread that the progress was logged from.
        /// </summary>
        public readonly int Thread { get; } = thread;
        /// <summary>
        /// The amount of items that were handled since the last log.
        /// </summary>
        public readonly int ItemshandledThisCycle { get; } = handledThisCycle;
        /// <summary>
        /// The progress of the serialization or deserialization.
        /// </summary>
        public readonly int Progress { get; } = progress;

        /// <summary>
        /// The progress of the serialization or deserialization as a percentage ranging from 0 to 1.
        /// </summary>
        public readonly float ProgressPercentage => (float)Progress / Total;
        /// <summary>
        /// The total amount of objects that will be serialized or deserialized on this thread.
        /// </summary>
        public readonly int Total { get; } = total;
        /// <summary>
        /// The message that was logged.
        /// </summary>
        public readonly string Message { get; } = message;
        /// <summary>
        /// Whether or not the serialization or deserialization has completed.
        /// </summary>
        public bool Completed => Progress == Total;

        public static implicit operator ProgressLog(string log) => new(-1, -1, -1, -1, log);

        public override string ToString()
        {
            return $"Thread: {Thread}, Progress: {Progress}, Total: {Total}, Message: {Message}";
        }
    }
}
