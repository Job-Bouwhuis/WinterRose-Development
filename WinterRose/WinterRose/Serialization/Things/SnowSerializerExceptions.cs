using System.Runtime.Serialization;
using System;
using WinterRose.Exceptions;

namespace WinterRose
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// Gets thrown when an argument is invalid
    /// </summary>
    [Serializable]
    public class InvalidArgumentException : WinterException
    {
        public InvalidArgumentException() { }
        public InvalidArgumentException(string message) : base(message) { }
        public InvalidArgumentException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Gets thrown when a type is not found when using the serializer
    /// </summary>
    [Serializable]
    public class TypeNotFoundException : WinterException
    {
        public TypeNotFoundException() { }
        public TypeNotFoundException(string message) : base(message) { }
        public TypeNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
    /// <summary>
    /// Gets thrown when a field is not supported by the serializer
    /// </summary>
    [Serializable]
    public class FieldNotSupportedException : WinterException
    {
        public FieldNotSupportedException() { }
        public FieldNotSupportedException(string message) : base(message) { }
        public FieldNotSupportedException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Gets thrown when a field is not found within the current handling class, but is present in the serialized data
    /// </summary>
    [Serializable]
    public class FieldNotFoundException : WinterException
    {
        public FieldNotFoundException() { }
        public FieldNotFoundException(string message) : base(message) { }
        public FieldNotFoundException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Gets thrown when anything fails when serializing
    /// </summary>
    [Serializable]
    public class SerializationFailedException : WinterException
    {
        public SerializationFailedException() { }
        public SerializationFailedException(string message) : base(message) { }
        public SerializationFailedException(Exception inner) : base("Serialization Failed. Check inner exception for details.", inner) { }
    }
    /// <summary>
    /// Gets thrown when anything fails when deserializing
    /// </summary>
    [Serializable]
    public class DeserializationFailedException : WinterException
    {
        public DeserializationFailedException() { }
        public DeserializationFailedException(string message) : base(message) { }
        public DeserializationFailedException(Exception inner) : base("Deserialization Failed. Check inner exception for details.", inner) { }
        public DeserializationFailedException(string message, Exception inner) : base(message, inner) { }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
