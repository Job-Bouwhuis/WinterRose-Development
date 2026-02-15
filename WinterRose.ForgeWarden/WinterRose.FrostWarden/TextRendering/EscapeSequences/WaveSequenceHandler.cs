namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using Raylib_cs;
using System;
using System.Collections.Generic;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Handles wave effect escape sequences: \[wave text] or \[w text;amplitude=3;speed=2;wavelength=15]
/// </summary>
public class WaveSequenceHandler : EscapeSequenceHandler
{
    public override string PrimaryKeyword => "wave";
    public override IEnumerable<string> AlternativeKeywords => new[] { "w" };

    public override int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition)
    {
        string textContent = content;
        float amplitude = 3f;
        float speed = 2f;
        float wavelength = 15f;

        // Parse parameters if provided (format: text;amplitude=3;speed=2;wavelength=15)
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
                        case "amplitude":
                            if (float.TryParse(value, out float amp))
                                amplitude = amp;
                            break;
                        case "speed":
                            if (float.TryParse(value, out float spd))
                                speed = spd;
                            break;
                        case "wavelength":
                            if (float.TryParse(value, out float wl))
                                wavelength = wl;
                            break;
                    }
                }
            }
        }

        elements.Add(new RichWave(textContent, currentColor, amplitude, speed, wavelength));
        return currentPosition;
    }
}
