using System;
using System.Collections.Generic;

namespace WinterRose.ForgeCodex.Parsing
{
    public sealed class Token
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public object? Literal { get; }
        public int Position { get; }

        public Token(TokenType type, string lexeme, object? literal, int position)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Position = position;
        }

        public override string ToString() => $"{Type} '{Lexeme}'";
    }
}