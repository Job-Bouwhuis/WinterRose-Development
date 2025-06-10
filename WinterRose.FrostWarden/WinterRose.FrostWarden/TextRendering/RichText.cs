namespace WinterRose.FrostWarden.TextRendering;

using Raylib_cs;
using System.Collections.Generic;
using System.Text;

public class RichText
{
    public List<RichElement> Elements { get; }

    public RichText(List<RichElement> elements)
    {
        Elements = elements;
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        foreach (var e in Elements)
            sb.Append(e.ToString());
        return sb.ToString();
    }

    public static RichText Parse(string text) => Parse(text, Color.White);
    
    public static RichText Parse(string text, Color defaultColor)
    {
        var elements = new List<RichElement>();
        Color currentColor = defaultColor;
        int i = 0;

        while (i < text.Length)
        {
            if (text[i] == '\\' && i + 1 < text.Length)
            {
                if (text[i + 1] == 'c' && text[i + 2] == '[')
                {
                    int close = text.IndexOf(']', i + 3);
                    if (close > 0)
                    {
                        string colorStr = text[(i + 3)..close];
                        currentColor = ParseColor(colorStr, defaultColor);
                        i = close + 1;
                        continue;
                    }
                }
                else if (text[i + 1] == 's' && text[i + 2] == '[')
                {
                   
                    int close = text.IndexOf(']', i + 3);
                    if (close > 0)
                    {
                        string spriteKey = text[(i + 3)..close];
                        RichSprite s;
                        elements.Add(s = new RichSprite(spriteKey, RichSpriteRegistry.GetSourceFor(spriteKey), 1f, currentColor));
                        i = close + 1;

                        if (text.Length >= close + 2)
                        {
                            if (text[close + 1] is '\\' && text[close + 2] is '!')
                            {
                                s.Clickable = true;
                                i += 2;
                            }
                        }

                        continue;
                    }
                    

                }
            }

            elements.Add(new RichGlyph(text[i], currentColor));
            i++;
        }

        return new RichText(elements);
    }

    public float MeasureText(Font? font, float fontSize)
    {
        font ??= ray.GetFontDefault();
        float width = 0;
        foreach (var element in Elements)
        {
            switch (element)
            {
                case RichGlyph glyph:
                    width += Raylib.MeasureTextEx(font.Value, glyph.Character.ToString(), fontSize, 1).X;
                    break;

                case RichSprite sprite:
                    var texture = RichSpriteRegistry.GetSprite(sprite.SpriteKey);
                    if (texture is not null)
                    {
                        float spriteHeight = sprite.BaseSize * fontSize;
                        float scale = spriteHeight / texture.Height;
                        width += texture.Width * scale;
                    }
                    break;
            }
        }
        return width;
    }

    private static Color ParseColor(string input, Color fallback)
    {
        if (input.StartsWith("#") && input.Length == 7)
        {
            // Hex RGB like #ff99cc
            return new Color(
                Convert.ToInt32(input.Substring(1, 2), 16),
                Convert.ToInt32(input.Substring(3, 2), 16),
                Convert.ToInt32(input.Substring(5, 2), 16),
                255
            );
        }

        // Simple color keywords
        return input.ToLower() switch
        {
            "red" => Color.Red,
            "blue" => Color.Blue,
            "green" => Color.Green,
            "white" => Color.White,
            "black" => Color.Black,
            "yellow" => Color.Yellow,
            _ => fallback
        };
    }
}
