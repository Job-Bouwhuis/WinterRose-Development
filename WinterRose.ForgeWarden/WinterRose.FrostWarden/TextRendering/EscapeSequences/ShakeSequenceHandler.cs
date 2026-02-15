namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using Raylib_cs;
using System.Collections.Generic;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Handles shake effect escape sequences: \[shake text] or \[s text;intensity=2;speed=10]
/// </summary>
public class ShakeSequenceHandler : EscapeSequenceHandler
{
    public override string PrimaryKeyword => "shake";
    public override IEnumerable<string> AlternativeKeywords => new[] { "sh" };

    public override int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition)
    {
        string textContent = content;
        float intensity = 2f;
        float speed = 10f;

        // Parse parameters if provided (format: text;intensity=2;speed=10)
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
                        case "intensity":
                            if (float.TryParse(value, out float intensity_val))
                                intensity = intensity_val;
                            break;
                        case "speed":
                            if (float.TryParse(value, out float speed_val))
                                speed = speed_val;
                            break;
                    }
                }
            }
        }

        elements.Add(new RichShake(textContent, currentColor, intensity, speed));
        return currentPosition;
    }
}
