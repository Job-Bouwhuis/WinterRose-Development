using System;
using System.Runtime.Serialization;

namespace WinterRose.Exceptions
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Allows for manually setting the source, message, and stacktrace of an exception.
    /// </summary>
    [Serializable]
    public class WinterException : Exception
    {

        public WinterException() : base() { }
        public WinterException(string message) : base(message) { }
        public WinterException(string message, Exception? innerException) : base(message, innerException) { }
        
        public WinterException WithStackTrace(string newStacktrace)
        {
            SetStackTrace(newStacktrace);
            return this;
        }

        string? stackTrace = null;
        string? message = null;
        string? source = null;

        /// <summary>
        /// The message of this exception.
        /// </summary>
        public override string Message => message ?? base.Message;
        /// <summary>
        /// The stacktrace of this exception.
        /// </summary>
        public override string? StackTrace => stackTrace ?? base.StackTrace;
        /// <summary>
        /// The source of this exception.
        /// </summary>
        public override string? Source => source ?? base.Source;

        public void SetStackTrace(string newStacktrace) => stackTrace = newStacktrace;
        public void SetMessage(string newMessage) => message = newMessage;
        public void SetSource(string source) => this.source = source;
    }
}


