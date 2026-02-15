namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using Raylib_cs;
using System.Collections.Generic;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Handles sprite escape sequences: \[sprite spriteKey] or \[s spriteKey]
/// Supports clickable sprites with \! modifier
/// </summary>
public class SpriteSequenceHandler : EscapeSequenceHandler
{
    public override string PrimaryKeyword => "sprite";
    public override IEnumerable<string> AlternativeKeywords => new[] { "s" };

    public override int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition)
    {
        string spriteKey = content;
        var source = RichSpriteRegistry.GetSourceFor(spriteKey);
        var sprite = new RichSprite(spriteKey, source, 1f, currentColor);

        // Check for clickable modifier: \!
        int nextPos = currentPosition;
        if (text.Length >= nextPos + 2 && text[nextPos] == '\\' && text[nextPos + 1] == '!')
        {
            sprite.Clickable = true;
            nextPos += 2;
        }

        elements.Add(sprite);
        return nextPos;
    }
}
