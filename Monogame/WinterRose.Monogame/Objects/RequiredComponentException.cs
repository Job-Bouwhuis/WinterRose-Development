using System;

namespace WinterRose.Monogame.Exceptions
{
    [Serializable]
    internal class RequiredComponentException : Exception
    {
        public RequiredComponentException()
        {
        }

        public RequiredComponentException(string? message) : base(message)
        {
        }

        public RequiredComponentException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}