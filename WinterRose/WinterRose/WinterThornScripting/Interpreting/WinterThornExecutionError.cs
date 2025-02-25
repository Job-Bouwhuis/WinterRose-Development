using System;
using System.Runtime.Serialization;
using WinterRose.WinterThornScripting;

namespace WinterRose.WinterThornScripting.Interpreting
{
    /// <summary>
    /// Thrown when the interpreter of <see cref="WinterThorn"/> encounters an error.
    /// </summary>
    /// <param name="error"></param>
    /// <param name="ErrorCode"></param>
    /// <param name="ErrorDescription"></param>
    [Serializable]
    public class WinterThornExecutionError(ThornError error, string ErrorCode, string ErrorDescription) : Exception(ErrorCode + " " + ErrorDescription)
    {
        public ThornError error = error;
        public string ErrorCode = ErrorCode;
        public string ErrorDescription = ErrorDescription;
    }
}