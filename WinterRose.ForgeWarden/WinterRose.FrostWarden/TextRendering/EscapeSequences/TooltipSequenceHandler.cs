namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using Raylib_cs;
using System.Collections.Generic;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Handles tooltip escape sequences: \[tooltip text|tooltip content] or \[tt text|content]
/// </summary>
public class TooltipSequenceHandler : EscapeSequenceHandler
{
    public override string PrimaryKeyword => "tooltip";
    public override IEnumerable<string> AlternativeKeywords => new[] { "tt" };

    public override int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition)
    {
        string displayText = content;
        string tooltipContent = "";

        // Parse format: text|tooltip_content
        int pipeIndex = content.IndexOf('|');
        if (pipeIndex >= 0)
        {
            displayText = content[..pipeIndex];
            tooltipContent = content[(pipeIndex + 1)..];
        }

        elements.Add(new RichTooltip(displayText, currentColor, tooltipContent));
        return currentPosition;
    }
}
