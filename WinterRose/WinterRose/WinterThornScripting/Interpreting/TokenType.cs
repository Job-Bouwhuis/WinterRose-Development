namespace WinterRose.WinterThornScripting.Interpreting
{
    /// <summary>
    /// A token type in the WinterScript language.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// An invalid token
        /// </summary>
        Invalid,
        /// <summary>
        /// Represents the "crystal" keyword.
        /// </summary>
        Class,
        /// <summary>
        /// Represents the "variables" keyword.
        /// </summary>
        Variables,
        /// <summary>
        /// Represents the "function" keyword.
        /// </summary>
        Function,
        /// <summary>
        /// Represents the "return" keyword.
        /// </summary>
        Return,
        /// <summary>
        /// Represents a generic keyword.
        /// </summary>
        Keyword,
        /// <summary>
        /// Represents the left brace "{"
        /// </summary>
        LeftBrace,
        /// <summary>
        /// Represents the right brace "}"
        /// </summary>
        RightBrace,
        /// <summary>
        /// Represents the ( in a function call.
        /// </summary>
        LeftParenthesis,
        /// <summary>
        /// Represents the ) in a function call.
        /// </summary>
        RightParenthesis,
        /// <summary>
        /// Represents a semicolon ";"
        /// </summary>
        Semicolon,
        /// <summary>
        /// Represents the equal sign "=" in variable definitions
        /// </summary>
        AssignVariable,
        /// <summary>
        /// Represents an identifier (variable or function name).
        /// </summary>
        Identifier,
        /// <summary>
        /// Reresents an operator ('+', '-', '==')
        /// </summary>
        Operator,
        /// <summary>
        /// Represents the + operator
        /// </summary>
        Modulus,
        /// <summary>
        /// Represents the * operator
        /// </summary>
        Multiplication,
        /// <summary>
        /// Represents the / operator
        /// </summary>
        Division,
        /// <summary>
        /// Represents the - operator
        /// </summary>
        Subtraction,
        IfClause,
        elseCondition,
        /// <summary>
        /// Represents a numeric value.
        /// </summary>
        Number,
        /// <summary>
        /// Represents a string literal.
        /// </summary>
        String,
        /// <summary>
        /// Represents a comma ","
        /// </summary>
        ArgumentSeperator,
        /// <summary>
        /// Represents the end of the file.
        /// </summary>
        Eof,
        /// <summary>
        /// Represents an error token.
        /// </summary>
        Error,
        Boolean,
        IsEqual,
        IsInequal,
        GreaterThanOrEqual,
        LessThanOrEqual,
        And,
        Or,
        GreaterThan,
        LessThan,
        Null,
        /// <summary>
        /// Represents the ! operator
        /// </summary>
        Not,
        Comment,
        Namespace,
        AccessControl,
        ElseClause,
        Loop,
        Break,
        Continue,
        New,
        Addition,
        Assignment,
        OpenBrace,
        /// <summary>
        /// [
        /// </summary>
        CloseBracket,
        ParameterType,
        FunctionParameter,
        Comma,
        OpenParenthesis,
        CloseParenthesis,
        Accessor,
        Constructor,
        WhileLoop,
        Goto,
        Label,
        Xor,
        /// <summary>
        /// ]
        /// </summary>
        OpenBracket,
    }
}