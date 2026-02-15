namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using System.Collections.Generic;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Abstract base class for handling escape sequences in rich text.
/// Provides a unified interface for parsing, creating elements, and supporting easy extension.
/// </summary>
public abstract class EscapeSequenceHandler
{
    /// <summary>
    /// Gets the primary keyword that triggers this escape sequence (e.g., "color", "sprite").
    /// </summary>
    public abstract string PrimaryKeyword { get; }

    /// <summary>
    /// Gets alternative keywords for this escape sequence, if any (e.g., "c" for "color").
    /// </summary>
    public abstract IEnumerable<string> AlternativeKeywords { get; }

    /// <summary>
    /// Attempts to parse and handle this escape sequence.
    /// </summary>
    /// <param name="content">The content between brackets, e.g., "red" for \[color red]</param>
    /// <param name="currentColor">The current color context from the rich text parser</param>
    /// <param name="elements">The list to add resulting elements to</param>
    /// <param name="text">The original text being parsed (for multi-part sequences)</param>
    /// <param name="currentPosition">Current position in the original text (for multi-part sequences)</param>
    /// <returns>The new position in the text after parsing, or -1 if parsing failed</returns>
    public abstract int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition);

    /// <summary>
    /// Checks if a given keyword matches this handler's primary or alternative keywords.
    /// </summary>
    public bool MatchesKeyword(string keyword)
    {
        if (keyword == PrimaryKeyword)
            return true;

        foreach (var alt in AlternativeKeywords)
        {
            if (keyword == alt)
                return true;
        }

        return false;
    }
}

