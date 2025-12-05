using System;
using System.Globalization;
using System.Text;

namespace WinterRose.ForgeCodex.Parsing;

public sealed class Scanner
{
    private readonly string source;
    private int index;
    private int length;

    private static readonly string[] Operators = new[]
    {
        "->", "==", "!=", ">=", "<=", "&&", "||", "+", "-", "*", "/", "%", ">", "<", "!", "^"
    };

    private readonly List<Token> buffer = new();
    private bool eofReached;

    public Token ReadToken()
    {
        // read from buffer first
        if (buffer.Count > 0)
        {
            var tok = buffer[0];
            buffer.RemoveAt(0);
            return tok;
        }

        return NextToken();
    }

    public Token PeekToken(int ahead)
    {
        if (ahead < 0)
            throw new ArgumentOutOfRangeException(nameof(ahead));

        EnsureBuffered(ahead);

        return buffer[ahead];
    }

    public Token? TryPeekToken(int ahead)
    {
        if (ahead < 0)
            return null;

        if (!EnsureBufferedSafe(ahead))
            return null;

        return buffer[ahead];
    }

    private void EnsureBuffered(int ahead)
    {
        while (buffer.Count <= ahead && !eofReached)
        {
            var tok = NextToken();
            buffer.Add(tok);
            if (tok.Type == TokenType.EndOfFile)
                eofReached = true;
        }

        if (ahead >= buffer.Count)
            throw new InvalidOperationException($"Cannot peek token {ahead}; end of stream reached.");
    }

    private bool EnsureBufferedSafe(int ahead)
    {
        while (buffer.Count <= ahead && !eofReached)
        {
            var tok = NextToken();
            buffer.Add(tok);
            if (tok.Type == TokenType.EndOfFile)
                eofReached = true;
        }

        return ahead < buffer.Count;
    }

    public Scanner(string source)
    {
        this.source = source ?? string.Empty;
        index = 0;
        length = this.source.Length;
    }

    private char Peek()
    {
        if (index >= length) return '\0';
        return source[index];
    }

    private char PeekNext()
    {
        if (index + 1 >= length) return '\0';
        return source[index + 1];
    }

    private char Advance()
    {
        if (index >= length) return '\0';
        return source[index++];
    }
    private bool Matches(string s)
    {
        if (index - 1 + s.Length > length) return false;
        for (int i = 0; i < s.Length; i++)
            if (source[index - 1 + i] != s[i]) return false;
        return true;
    }

    // helper to advance by N chars
    private void Advance(int n)
    {
        index += n;
    }

    private void SkipWhitespace()
    {
        while (char.IsWhiteSpace(Peek())) Advance();
    }

    private Token NextToken()
    {
        SkipWhitespace();
        int start = index;
        char c = Advance();
        if (c == '\0') return new Token(TokenType.EndOfFile, string.Empty, null, index);

        // symbols
        if (c == '{') return new Token(TokenType.LBrace, "{", null, start);
        if (c == '}') return new Token(TokenType.RBrace, "}", null, start);
        if (c == '(') return new Token(TokenType.LParen, "(", null, start);
        if (c == ')') return new Token(TokenType.RParen, ")", null, start);
        if (c == ',') return new Token(TokenType.Comma, ",", null, start);
        if (c == ':') return new Token(TokenType.Colon, ":", null, start);
        if (c == '[') return new Token(TokenType.LBracket, "[", null, start);
        if (c == ']') return new Token(TokenType.RBracket, "]", null, start);

        // string literal
        if (c == '"' || c == '\'')
        {
            char quote = c;
            var sb = new StringBuilder();
            while (Peek() != '\0' && Peek() != quote)
            {
                if (Peek() == '\\')
                {
                    Advance();
                    char esc = Advance();
                    sb.Append(esc switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        _ => esc
                    });
                }
                else
                {
                    sb.Append(Advance());
                }
            }
            if (Peek() == quote) Advance();
            return new Token(TokenType.String, sb.ToString(), sb.ToString(), start);
        }

        // multi-character operators first (special-case arrow ->)
        foreach (var op in Operators.OrderByDescending(x => x.Length))
        {
            if (Matches(op))
            {
                Advance(op.Length - 1); // already consumed first char
                if (op == "->")
                    return new Token(TokenType.Arrow, "->", null, start);
                return new Token(TokenType.Operator, op, null, start);
            }
        }

        // number literal
        if (char.IsDigit(c))
        {
            var sb = new StringBuilder();
            sb.Append(c);
            while (char.IsDigit(Peek())) sb.Append(Advance());
            if (Peek() == '.')
            {
                sb.Append(Advance());
                while (char.IsDigit(Peek())) sb.Append(Advance());
            }
            string numText = sb.ToString();
            if (double.TryParse(numText, NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
                return new Token(TokenType.Number, numText, d, start);
            return new Token(TokenType.Number, numText, null, start);
        }

        // identifier, keyword, boolean (allow generics like Auto<int> as single identifier)
        if (char.IsLetter(c) || c == '_')
        {
            var sb = new StringBuilder();
            sb.Append(c);
            while (char.IsLetterOrDigit(Peek()) || Peek() == '_') sb.Append(Advance());

            // if this identifier is followed by generics '<...>' capture the whole balanced generic content
            if (Peek() == '<')
            {
                int depth = 0;
                do
                {
                    char ch = Advance();
                    sb.Append(ch);
                    if (ch == '<') depth++;
                    else if (ch == '>') depth--;
                    if (depth == 0) break;
                    if (Peek() == '\0') throw new InvalidOperationException($"Malformed generic definition '{sb}'");
                } while (true);
            }

            string text = sb.ToString();

            // map keywords to token types
            switch (text.ToLowerInvariant())
            {
                case "from": return new Token(TokenType.KeywordFrom, text, null, start);
                case "where": return new Token(TokenType.KeywordWhere, text, null, start);
                case "take": return new Token(TokenType.KeywordTake, text, null, start);
                case "add": return new Token(TokenType.KeywordAdd, text, null, start);
                case "to": return new Token(TokenType.KeywordTo, text, null, start);
                case "remove": return new Token(TokenType.KeywordRemove, text, null, start);
                case "update": return new Token(TokenType.KeywordUpdate, text, null, start);
                case "create": return new Token(TokenType.KeywordCreate, text, null, start);
                case "drop": return new Token(TokenType.KeywordDrop, text, null, start);
                case "table": return new Token(TokenType.KeywordTable, text, null, start);
                case "for": return new Token(TokenType.KeywordFor, text, null, start);
                case "if": return new Token(TokenType.KeywordIf, text, null, start);
                case "then": return new Token(TokenType.KeywordThen, text, null, start);
                case "else": return new Token(TokenType.KeywordElse, text, null, start);
                case "except": return new Token(TokenType.KeywordExcept, text, null, start);
                case "order": return new Token(TokenType.KeywordOrder, text, null, start);
                case "by": return new Token(TokenType.KeywordBy, text, null, start);
                case "limit": return new Token(TokenType.KeywordLimit, text, null, start);
                case "count": return new Token(TokenType.KeywordCount, text, null, start);
                case "exists": return new Token(TokenType.KeywordExists, text, null, start);
                case "or": return new Token(TokenType.KeywordOr, text, null, start);
                case "descending": return new Token(TokenType.KeywordDescending, text, null, start);
                default:
                    // boolean literals
                    if (string.Equals(text, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "false", StringComparison.OrdinalIgnoreCase))
                        return new Token(TokenType.Boolean, text, bool.Parse(text), start);

                    return new Token(TokenType.Identifier, text, text, start);
            }
        }

        throw new Exception($"Unexpected character '{c}' at {start}");
    }
}
