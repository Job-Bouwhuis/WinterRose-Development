using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.WinterThornScripting;

namespace WinterRose.Expressions
{
    [Serializable]
    internal class MathSolverException : Exception
    {
        string message;
        public override string Message => message;

        public MathSolverException(List<Token> tokens)
        {
            StringBuilder sb = new();
            tokens.Foreach(token => sb.Append(token.Identifier).Append(' '));
            message = "Failed to parse expression: " + sb.ToString();
        }
    }
}