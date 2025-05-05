using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Exceptions;

namespace WinterRose.CrystalScripting.Legacy.Objects.Base
{
    [Serializable]
    public sealed class CrystalError : WinterException
    {
        public static CrystalError NoError { get; } = new CrystalError("None", "No Errors Documented");
        string errorName;
        string errorMessage;
        CrystalError stackTrace;

        public string ErrorName { get => errorName; set => errorName = value; }
        public string ErrorMessage { get => errorMessage; set => errorMessage = value; }
        public CrystalError StackTrace { get => stackTrace; set => stackTrace = value; }

        public CrystalError(string errorName, string errorMessage, CrystalError stackTrace)
        {
            this.errorName = errorName;
            this.errorMessage = errorMessage;
            this.stackTrace = stackTrace;
        }

        public CrystalError(string errorName, string errorMessage)
        {
            this.errorName = errorName;
            this.errorMessage = errorMessage;
        }

        public override string ToString()
        {
            return $"{errorName}: {errorMessage}";
        }
    }
}


