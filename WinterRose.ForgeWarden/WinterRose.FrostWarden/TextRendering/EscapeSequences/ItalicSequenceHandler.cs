namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using Raylib_cs;
using System.Collections.Generic;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Handles italic text escape sequences: \[italic text] or \[i text]
/// </summary>
public class ItalicSequenceHandler : EscapeSequenceHandler
{
    public override string PrimaryKeyword => "italic";
    public override IEnumerable<string> AlternativeKeywords => new[] { "i" };

    public override int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition)
    {
        elements.Add(new RichItalic(content, currentColor));
        return currentPosition;
    }
}
