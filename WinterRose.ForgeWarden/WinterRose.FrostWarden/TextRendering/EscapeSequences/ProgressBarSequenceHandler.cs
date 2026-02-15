namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using Raylib_cs;
using System.Collections.Generic;
using System.Globalization;
using WinterRose.ForgeWarden.TextRendering.RichElements;

/// <summary>
/// Handles progress bar escape sequences: \[progress value=0.5] or \[progress value=50 max=100]
/// </summary>
public class ProgressBarSequenceHandler : EscapeSequenceHandler
{
    public override string PrimaryKeyword => "progress";
    public override IEnumerable<string> AlternativeKeywords => new[] { "prog" };

    public override int Parse(string content, Raylib_cs.Color currentColor, List<RichElement> elements, string text, int currentPosition)
    {
        float value = 0.5f;
        float? maxValue = 1f;
        float width = 100f;

        // Parse parameters (format: value=50 max=100 width=100)
        if (!string.IsNullOrEmpty(content))
        {

            var parameters = content.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var param in parameters)
            {
                var kv = param.Split('=', 2);
                if (kv.Length == 2)
                {
                    string key = kv[0].Trim().ToLowerInvariant();
                    string val = kv[1].Trim();
                    string normalized = val.Replace(',', '.');
                    switch (key)
                    {
                        case "value":
                            if (float.TryParse(
                                normalized,
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture,
                                out float v))
                                value = v;
                            break;
                        case "max":
                            if (float.TryParse(
                              normalized,
                              NumberStyles.Float,
                              CultureInfo.InvariantCulture,
                              out float m))
                                maxValue = m;
                            break;
                        case "width":
                            if (float.TryParse(
                              normalized,
                              NumberStyles.Float,
                              CultureInfo.InvariantCulture,
                              out float w))
                                width = w;
                            break;
                    }
                }
            }
        }

        var bar = new RichProgressBar(value, maxValue, width)
        {
            FillColor = currentColor
        };
        elements.Add(bar);
        return currentPosition;
    }
}
