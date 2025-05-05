using System.Diagnostics;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.WinterThornScripting
{
    /// <summary>
    /// A token in the WinterScript language.
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="type"></param>
    [DebuggerDisplay("Token: {Identifier} ({Type})")]
    [method: DefaultArguments("", TokenType.Invalid)]
    public class Token(string identifier, TokenType type)
    {
        /// <summary>
        /// The identifier of this token.
        /// </summary>
        [IncludeWithSerialization]
        public string Identifier { get; private set; } = identifier;
        /// <summary>
        /// The type of this token.
        /// </summary>
        [IncludeWithSerialization]
        public TokenType Type { get; private set; } = type;
    }
}