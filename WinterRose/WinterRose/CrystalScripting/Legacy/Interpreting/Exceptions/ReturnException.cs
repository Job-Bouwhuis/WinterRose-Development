using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.CrystalScripting.Legacy.Objects.Base;
using WinterRose.Exceptions;

namespace WinterRose.CrystalScripting.Legacy.Interpreting.Exceptions
{

    [Serializable]
    public class ReturnException : WinterException
    {
        public readonly CrystalVariable returnValue;

        public ReturnException() { }
        public ReturnException(CrystalVariable value) : base("Return Keyword Used In Crystal Method") { returnValue = value; }
        public ReturnException(string message, Exception inner) : base(message, inner) { }
    }
}
