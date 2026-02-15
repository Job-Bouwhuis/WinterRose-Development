namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using Raylib_cs;
using System.Collections.Generic;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Handles bold text escape sequences: \[bold text] or \[b text]
/// </summary>
public class BoldSequenceHandler : EscapeSequenceHandler
{
    public override string PrimaryKeyword => "bold";
    public override IEnumerable<string> AlternativeKeywords => new[] { "b" };

    public override int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition)
    {
        elements.Add(new RichBold(content, currentColor));
        return currentPosition;
    }
}
