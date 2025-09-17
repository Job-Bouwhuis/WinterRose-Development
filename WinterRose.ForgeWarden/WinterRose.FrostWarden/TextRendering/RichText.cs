namespace WinterRose.ForgeWarden.TextRendering;

using Raylib_cs;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq.Expressions;
using System.Text;
using WinterRose.WinterThornScripting.Interpreting;

public class RichText
{
    public Font Font { get; set; } = ray.GetFontDefault();
    public float Spacing { get; set; } = 2;
    public int FontSize { get; set; } = 12;
    public List<RichElement> Elements { get; private set; }

    public Color lastColorInSequence
    {
        get
        {
            for (int i = Elements.Count; i >= 0; i--)
            {
                RichElement? element = Elements[i];
                if (element is RichGlyph glyph)
                    return glyph.Color;
                if (element is RichSprite sprite)
                    return sprite.Tint;
            }
            return Color.White;
        }
    }

    public void SetText(RichText title, bool copyMetadata = false)
    {
        Elements = title.Elements;

        if(copyMetadata)
        {
            Font = title.Font;
            Spacing = title.Spacing;
            FontSize = title.FontSize;
        }
    }

    public static implicit operator RichText(string text)
    {
        return Parse(text, Color.White);
    }

    public static RichText operator +(RichText a, RichText b)
    {
        var combinedElements = new List<RichElement>(a.Elements);
        combinedElements.AddRange(b.Elements);
        return new RichText(combinedElements) { Font = a.Font, Spacing = a.Spacing, FontSize = a.FontSize };
    }

    public static RichText operator +(RichText a, RichElement b)
    {
        var combinedElements = new List<RichElement>(a.Elements) { b };
        return new RichText(combinedElements) { Font = a.Font, Spacing = a.Spacing, FontSize = a.FontSize };
    }

    public static RichText operator +(RichElement a, RichText b)
    {
        var combinedElements = new List<RichElement> { a };
        combinedElements.AddRange(b.Elements);
        return new RichText(combinedElements) { Font = b.Font, Spacing = b.Spacing, FontSize = b.FontSize };
    }

    public static RichText operator +(RichText a, string b)
    {
        var combinedElements = new List<RichElement>(a.Elements);
        combinedElements.AddRange(RichText.Parse(b).Elements);
        return new RichText(combinedElements) { Font = a.Font, Spacing = a.Spacing, FontSize = a.FontSize };
    }

    public static RichText operator +(string a, RichText b)
    {
        var combinedElements = new List<RichElement>(RichText.Parse(a).Elements);
        combinedElements.AddRange(b.Elements);
        return new RichText(combinedElements) { Font = b.Font, Spacing = b.Spacing, FontSize = b.FontSize };
    }

    public static RichText operator +(RichText a, char b)
    {

        var combinedElements = new List<RichElement>(a.Elements) { new RichGlyph(b,  a.lastColorInSequence)};
        return new RichText(combinedElements) { Font = a.Font, Spacing = a.Spacing, FontSize = a.FontSize };
    }

    public static RichText operator +(char a, RichText b)
    {
        var combinedElements = new List<RichElement> { new RichGlyph(a, b.lastColorInSequence) };
        combinedElements.AddRange(b.Elements);
        return new RichText(combinedElements) { Font = b.Font, Spacing = b.Spacing, FontSize = b.FontSize };
    }

    public static RichText operator +(RichText a, object? o)
    {
        return a + (o?.ToString() ?? "{null}");
    }

    public Rectangle CalculateBounds(float maxWidth)
    {
        var lines = RichTextRenderer.WrapText(this, maxWidth);

        List<Rectangle> lineSizes = [];

        foreach (var line in lines)
            lineSizes.Add(CalculateLineSize(line));
        float width = lineSizes.Count > 0 ? lineSizes.Max(r => r.Width) : 0;
        return new Rectangle(0, 0, width, lines.Count * (FontSize) + lines.Count * Spacing);
    }

    internal Rectangle CalculateLineSize(List<RichElement> elements)
    {
        Rectangle r = new();
        bool first = true;
        foreach (RichElement element in elements)
        {
            if (element is RichGlyph rg)
            {
                var size = ray.MeasureTextEx(Font, "" + rg.Character, FontSize, Spacing);

                if (first)
                {
                    first = false;
                    r.Height = size.Y;
                }
                if (size.X > 0)
                    r.Width += size.X + Spacing;
                if (rg.Character is '\n')
                    r.Height += size.Y;
                continue;
            }

            if (element is RichSprite rs && RichSpriteRegistry.GetSprite(rs.SpriteKey) is Sprite s)
            {
                float spriteHeight = rs.BaseSize * FontSize;
                float scale = spriteHeight / s.Height;
                r.Width += s.Width * scale + Spacing;
                continue;
            }
        }

        if (r.X >= Spacing)
            r.X -= Spacing;
        return r;
    }

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

    public static RichText Parse(string text, int fontSize = 12) => Parse(text, Color.White, fontSize);
    
    public static RichText Parse(string text, Color defaultColor, int fontSize = 12)
    {
        var elements = new List<RichElement>();
        Color currentColor = defaultColor;
        int i = 0;

        while (i < text.Length)
        {
            try
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
                    else if (text[i + 1] == 'L' && text[i + 2] == '[')
                    {
                        int close = text.IndexOf(']', i + 3);
                        if (close > 0)
                        {
                            string content = text[(i + 3)..close];
                            string linkUrl = content;
                            string displayText = content;

                            int eq = content.IndexOf('|');
                            if (eq >= 0)
                            {
                                linkUrl = content[..eq];
                                displayText = content[(eq + 1)..];
                            }

                            // Add each character as a RichGlyph but attach link metadata
                            foreach (char c in displayText)
                            {
                                elements.Add(new RichGlyph(c, currentColor)
                                {
                                    GlyphLinkUrl = linkUrl
                                });
                            }

                            i = close + 1;
                            continue;
                        }
                    }
                }
            }
            catch
            {
                // ignore, and draw characters as normal if anything here fails
            }

            elements.Add(new RichGlyph(text[i], currentColor));
            i++;
        }

        return new RichText(elements) {
            FontSize = fontSize
        };
    }

    public float MeasureText(Font? font)
    {
        font ??= ray.GetFontDefault();
        float width = 0;
        foreach (var element in Elements)
        {
            switch (element)
            {
                case RichGlyph glyph:
                    width += Raylib.MeasureTextEx(font.Value, glyph.Character.ToString(), FontSize, 1).X;
                    break;

                case RichSprite sprite:
                    var texture = RichSpriteRegistry.GetSprite(sprite.SpriteKey);
                    if (texture is not null)
                    {
                        float spriteHeight = sprite.BaseSize * FontSize;
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

    public RichText Clone()
    {
        List<RichElement> clonedElements = new();

        foreach (var element in Elements)
        {
            if (element is RichGlyph glyph)
            {
                clonedElements.Add(new RichGlyph(glyph.Character, glyph.Color));
            }
            else if (element is RichSprite sprite)
            {
                clonedElements.Add(new RichSprite(sprite.SpriteKey, sprite.SpriteSource, sprite.BaseSize, sprite.Tint)
                {
                    Clickable = sprite.Clickable
                });
            }
        }

        return new RichText(clonedElements)
        {
            Font = this.Font,
            Spacing = this.Spacing,
            FontSize = this.FontSize
        };
    }
}
