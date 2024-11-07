using System;
using System.Runtime.Serialization;

namespace WinterRose;

/// <summary>
/// Something was not found
/// </summary>
[Serializable]
public class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string? message) : base(message)
    {
    }

    public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}