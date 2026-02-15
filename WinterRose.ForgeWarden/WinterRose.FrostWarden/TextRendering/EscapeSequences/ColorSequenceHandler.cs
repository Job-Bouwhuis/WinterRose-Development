namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using Raylib_cs;
using System.Collections.Generic;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Handles color escape sequences: \[color red] or \[c red]
/// Note: This handler is special as it updates the parsing context rather than creating elements.
/// It's handled directly in RichText.Parse for immediate color state updates.
/// </summary>
public class ColorSequenceHandler : EscapeSequenceHandler
{
    public override string PrimaryKeyword => "color";
    public override IEnumerable<string> AlternativeKeywords => new[] { "c" };

    public override int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition)
    {
        return currentPosition;
    }
}

