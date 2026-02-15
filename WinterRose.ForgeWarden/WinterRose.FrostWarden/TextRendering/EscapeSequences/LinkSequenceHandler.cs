namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using Raylib_cs;
using System.Collections.Generic;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Handles link escape sequences: \[link url] or \[link url|displayText] or \[L ...]
/// Creates RichWord elements with LinkUrl set
/// </summary>
public class LinkSequenceHandler : EscapeSequenceHandler
{
    public override string PrimaryKeyword => "link";
    public override IEnumerable<string> AlternativeKeywords => new[] { "L" };

    public override int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition)
    {
        string linkUrl = content;
        string displayText = content;

        // Parse pipe-separated format: url|displayText
        int pipeIndex = content.IndexOf('|');
        if (pipeIndex >= 0)
        {
            linkUrl = content[..pipeIndex];
            displayText = content[(pipeIndex + 1)..];
        }

        // Split display text into words and add as RichWord elements
        var parts = displayText.Split(' ');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
                elements.Add(new RichWord(parts[i], currentColor, linkUrl));

            // Add space glyphs between words (but not after the last word)
            if (i < parts.Length - 1)
                elements.Add(new RichGlyph(' ', currentColor));
        }

        return currentPosition;
    }
}
