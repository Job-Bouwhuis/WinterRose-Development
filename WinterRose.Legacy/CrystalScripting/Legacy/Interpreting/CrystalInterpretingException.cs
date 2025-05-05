using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Exceptions;

namespace WinterRose.CrystalScripting.Legacy.Interpreting
{

    [Serializable]
    public class CrystalInterpretingException : WinterException
    {
        public readonly int ExceptionCode;

        public CrystalInterpretingException() { ExceptionCode = 0; }
        public CrystalInterpretingException(int code, string message) : base(message) { }
        public CrystalInterpretingException(int code, string message, Exception inner) : base(message, inner) { }
    }
}
