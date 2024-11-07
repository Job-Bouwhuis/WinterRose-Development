namespace WinterRose.CrystalScripting.Legacy.Interpreting
{
    /// <summary>
    /// Represents the type of a token in the CrystalScript language.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Represents the "crystal" keyword.
        /// </summary>
        Crystal,
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
        Addition,
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
        ifCondition,
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
        Not
    }
}


