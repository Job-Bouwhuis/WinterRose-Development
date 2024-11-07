using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Serialization;

namespace WinterRose.CrystalScripting.Legacy.Interpreting
{
    [DebuggerDisplay("Lexeme: {Lexeme} --- Type: {Type}"), IncludePrivateFields]
    public class Token
    {
        internal object argument;

        public object Argument { get; set; }
        public TokenType Type { get; }
        public string Lexeme { get; }
        public int Start { get; }

        [DefaultArguments(TokenType.Error, "newly created", 0)]
        public Token(TokenType type, string lexeme, int start)
        {
            Type = type;
            Lexeme = lexeme;
            Start = start;
        }

        public bool IsKeyword()
        {
            return Type is TokenType.Crystal
                or TokenType.Variables
                or TokenType.Function
                or TokenType.Return
                or TokenType.ifCondition
                or TokenType.Null;
        }

        public bool IsPunctuation()
        {
            return Type is TokenType.LeftBrace
                or TokenType.RightBrace
                or TokenType.Semicolon
                or TokenType.AssignVariable;
        }

        public bool IsOperator()
        {
            return Type is TokenType.Addition
            or TokenType.Subtraction
            or TokenType.Multiplication
            or TokenType.Division
            or TokenType.IsEqual
            or TokenType.IsInequal
            or TokenType.GreaterThan
            or TokenType.LessThan
            or TokenType.GreaterThanOrEqual
            or TokenType.LessThanOrEqual
            or TokenType.And
            or TokenType.Or;
        }

        public bool IsNumber()
        {
            return Type is TokenType.Number;
        }

        public bool IsString()
        {
            return Type is TokenType.String;
        }

        public bool IsIdentifier()
        {
            return Type is TokenType.Identifier;
        }

        public static bool IsKeyword(string lexeme)
        {
            return lexeme is "crystal"
                or "variables"
                or "function"
                or "return"
                or "if"
                or "else"

                or "number"
                or "string"
                or "bool"

                or "null"
                or "true"
                or "false";
        }

        public static bool IsPunctuation(string lexeme)
        {
            return lexeme
                is "{"
                or "}"
                or ";"
                or "=";
        }

        public static bool IsOperator(string lexeme)
        {
            return lexeme
                is "+"
                or "-"
                or "*"
                or "/"
                or "=="
                or "!="
                or "<="
                or ">="
                or ">"
                or "<";
        }

        public static bool IsNumber(string lexeme)
        {
            return double.TryParse(lexeme, out _);
        }

        public static bool IsString(string lexeme)
        {
            return lexeme.StartsWith("\"") && lexeme.EndsWith("\"");
        }

        internal bool IsMathOperator()
        {
            return Type is TokenType.Addition
                or TokenType.Subtraction
                or TokenType.Multiplication
                or TokenType.Division;
        }
        public override string ToString()
        {
            return $"Lexeme: {Lexeme} --- Type: {Type}";
        }
    }
}
