using System;
using System.Runtime.Serialization;
using WinterRose.Exceptions;

namespace WinterRose.Asserting
{
    [Serializable]
    internal class WinterAssertException : WinterException
    {
        public WinterAssertException()
        {
        }

        public WinterAssertException(string message) : base(message)
        {
        }

        public WinterAssertException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}