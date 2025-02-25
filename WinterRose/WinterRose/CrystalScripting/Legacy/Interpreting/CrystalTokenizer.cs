using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.CrystalScripting.Legacy.Interpreting
{
    public class CrystalTokenizer
    {
        private readonly string source;
        private int position
        {
            get
            {
                return p;
            }
            set
            {
                p = value;
            }
        }
        private int p;

        public List<Token> Tokens { get; private set; }

        public CrystalTokenizer(string source)
        {
            this.source = source;
            position = 0;
            Tokenize();
        }

        private List<Token> Tokenize()
        {
            Tokens = new List<Token>();

            while (!IsEnd())
            {
                char currentChar = Advance();

                if (char.IsWhiteSpace(currentChar))
                {
                    // Skip whitespace
                    continue;
                }
                else if (char.IsLetter(currentChar))
                {
                    Tokens.Add(ScanIdentifierOrKeyword());
                }
                else if (char.IsDigit(currentChar))
                {
                    Tokens.Add(ScanNumberLiteral());
                }
                else if (currentChar == '"')
                {
                    Tokens.Add(ScanStringLiteral());
                }
                else
                {
                    // operator
                    string lexeme = currentChar.ToString();
                    char next = PeekNext();
                    if (next is '=')
                    {
                        lexeme += next;
                        Advance();
                    }
                    if (Token.IsOperator(lexeme))
                        Tokens.Add(new Token(ScanOperator(lexeme), lexeme, position - 1));


                    else if (IsPunctuation(currentChar))
                    {
                        Tokens.Add(ScanPunctuation(currentChar));
                    }


                    // Unknown character, raise an error or handle it as needed
                }
            }

            return Tokens;
        }

        private char PeekNext()
        {
            char currentChar = Advance();
            position--;
            return currentChar;
        }

        private TokenType ScanOperator(string op)
        {
            //if(operatorChar == "=")
            //    return TokenType.Assignment;
            //else if(operatorChar == "+=")
            //    return TokenType.AdditionAssignment;
            //else if(operatorChar == "-=")
            //    return TokenType.SubtractionAssignment;
            //else if(operatorChar == "*=")
            //    return TokenType.MultiplicationAssignment;
            //else if(operatorChar == "/=")
            //    return TokenType.DivisionAssignment;
            //else if(operatorChar == "%=")
            //    return TokenType.ModuloAssignment;
            //else if(operatorChar == "++")
            //    return TokenType.Increment;
            //else if(operatorChar == "--")
            //    return TokenType.Decrement;
            if (op == "+")
                return TokenType.Addition;
            else if (op == "-")
                return TokenType.Subtraction;
            else if (op == "*")
                return TokenType.Multiplication;
            else if (op == "/")
                return TokenType.Division;
            //else if(operatorChar == "%")
            //    return TokenType.Modulo;
            //else if(operatorChar == "!")
            //    return TokenType.Not;
            else if (op == "==")
                return TokenType.IsEqual;
            else if (op == "!=")
                return TokenType.IsInequal;
            else if (op == ">")
                return TokenType.GreaterThan;
            else if (op == "<")
                return TokenType.LessThan;
            else if (op == ">=")
                return TokenType.GreaterThanOrEqual;
            else if (op == "<")
                return TokenType.LessThanOrEqual;
            else if (op == "<=")
                return TokenType.LessThanOrEqual;
            return TokenType.Error;
        }

        private Token ScanIdentifierOrKeyword()
        {
            int start = position - 1;
            StringBuilder lexemeBuilder = new StringBuilder();

            while (!IsEnd() && (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_'))
            {
                lexemeBuilder.Append(CurrentChar);
                Advance();
            }

            string lexeme = lexemeBuilder.ToString();
            TokenType type;

            if (Token.IsKeyword(lexeme))
            {
                type = TokenType.Keyword;
            }
            else
            {
                type = TokenType.Identifier;
            }
            position--;
            return new Token(type, lexeme, start);
        }

        private Token ScanNumberLiteral()
        {
            int start = position - 1;
            StringBuilder lexemeBuilder = new StringBuilder();

            while (!IsEnd() && char.IsDigit(CurrentChar))
            {
                lexemeBuilder.Append(CurrentChar);
                Advance();
            }

            if (!IsEnd() && CurrentChar == '.')
            {
                lexemeBuilder.Append(Advance());

                while (!IsEnd() && char.IsDigit(CurrentChar))
                {
                    lexemeBuilder.Append(Advance());
                }
            }

            string lexeme = lexemeBuilder.ToString();
            position--;
            return new Token(TokenType.Number, lexeme, start);
        }

        private Token ScanStringLiteral()
        {
            int start = position - 1;
            StringBuilder lexemeBuilder = new StringBuilder();
            bool escapeNextChar = false;

            //Advance();

            while (!IsEnd())
            {
                char currentChar = Advance();

                if (escapeNextChar)
                {
                    lexemeBuilder.Append(currentChar);
                    escapeNextChar = false;
                }
                else if (currentChar == '\\')
                {
                    escapeNextChar = true;
                }
                else if (currentChar == '"')
                {
                    break;
                }
                else
                {
                    lexemeBuilder.Append(currentChar);
                }
            }

            string lexeme = lexemeBuilder.ToString();
            //Advance();
            return new Token(TokenType.String, $"\"{lexeme}\"", start);
        }

        private Token ScanPunctuation(char current)
        {
            try
            {
                int start = position - 1;

                // Map the punctuation character to the corresponding token type
                TokenType tokenType;
                switch (current)
                {
                    case '{':
                        tokenType = TokenType.LeftBrace;
                        break;
                    case '}':
                        tokenType = TokenType.RightBrace;
                        break;
                    case '(':
                        tokenType = TokenType.LeftParenthesis;
                        break;
                    case ')':
                        tokenType = TokenType.RightParenthesis;
                        break;
                    case ';':
                        tokenType = TokenType.Semicolon;
                        break;
                    case '=':
                        tokenType = TokenType.AssignVariable;
                        break;
                    case ',':
                        tokenType = TokenType.ArgumentSeperator;
                        break;
                    default:
                        // If the character is not recognized as a punctuation, throw an error
                        string errorMessage = $"Unexpected character '{CurrentChar}'";
                        return new Token(TokenType.Error, errorMessage, start);
                }

                return new Token(tokenType, current.ToString(), start);
            }
            finally
            {
                //Advance();
            }
        }

        private bool IsPunctuation(char c)
        {
            return "{}();,=".Contains(c);
        }

        private char CurrentChar
        {
            get { return source[position - 1]; }
        }

        private char Advance()
        {
            if (position + 1 >= source.Length)
            {
                return '\0';
            }
            return source[position++];
        }

        private bool IsEnd()
        {
            return position >= source.Length || position + 1 >= source.Length;
        }

        internal List<Token> GetRange(int start, int count)
        {
            return Tokens.GetRange(start, count);
        }
    }
}

