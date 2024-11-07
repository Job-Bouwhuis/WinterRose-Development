using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.WinterThornScripting;

namespace WinterRose.Expressions
{
    [Serializable]
    internal class booleanSolverException : Exception
    {
        string message;
        public override string Message => message;

        public booleanSolverException(List<Token> tokens)
        {
            StringBuilder sb = new();
            tokens.Foreach(token => sb.Append(token.Identifier).Append(' '));
            message = "Failed to parse expression: " + sb.ToString();
        }
    }
}