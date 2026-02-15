namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using Raylib_cs;
using System.Collections.Generic;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Handles typewriter reveal effect escape sequences: \[typewriter text] or \[tw text;delay=0.1]
/// </summary>
public class TypewriterSequenceHandler : EscapeSequenceHandler
{
    public override string PrimaryKeyword => "typewriter";
    public override IEnumerable<string> AlternativeKeywords => new[] { "tw" };

    public override int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition)
    {
        string textContent = content;
        float characterDelay = 0.1f;

        // Parse parameters if provided (format: text;delay=0.1)
        if (content.Contains(';'))
        {
            var parts = content.Split(';');
            textContent = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                var param = parts[i];
                if (param.Contains('='))
                {
                    var kv = param.Split('=');
                    string key = kv[0].Trim().ToLowerInvariant();
                    string value = kv[1].Trim();

                    switch (key)
                    {
                        case "delay":
                            if (float.TryParse(value, out float delay_val))
                                characterDelay = delay_val;
                            break;
                    }
                }
            }
        }

        elements.Add(new RichTypewriter(textContent, currentColor, characterDelay));
        return currentPosition;
    }
}
