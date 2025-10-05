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
            if (Elements == null || Elements.Count == 0) return Color.White;
            for (int i = Elements.Count - 1; i >= 0; i--)
            {
                RichElement element = Elements[i];
                if (element is RichGlyph glyph) return glyph.Color;
                if (element is RichSprite sprite) return sprite.Tint;
                if (element is RichWord word) return word.Color;
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
            if (element is RichWord rw)
            {
                var size = ray.MeasureTextEx(Font, rw.Text, FontSize, Spacing);
                if (first)
                {
                    first = false;
                    r.Height = size.Y;
                }
                if (size.X > 0)
                    r.Width += size.X + Spacing;
                continue;
            }

            if (element is RichGlyph rg)
            {
                var size = ray.MeasureTextEx(Font, rg.Character.ToString(), FontSize, Spacing);
                if (first)
                {
                    first = false;
                    r.Height = size.Y;
                }
                if (rg.Character == '\n')
                {
                    r.Height += size.Y;
                }
                else if (size.X > 0)
                {
                    r.Width += size.X + Spacing;
                }
                continue;
            }

            if (element is RichSprite rs && RichSpriteRegistry.GetSprite(rs.SpriteKey) is Sprite s)
            {
                float spriteHeight = rs.BaseSize * FontSize;
                float scale = spriteHeight / s.Height;
                r.Width += s.Width * scale + Spacing;
                if (first)
                {
                    first = false;
                    r.Height = Math.Max(r.Height, s.Height * scale);
                }
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
        var sb = new StringBuilder(); // accumulates a word
        void FlushWord(string? linkUrl = null)
        {
            if (sb.Length == 0) return;
            elements.Add(new RichWord(sb.ToString(), currentColor, linkUrl));
            sb.Clear();
        }

        int i = 0;
        while (i < text.Length)
        {
            try
            {
                if (text[i] == '\\' && i + 1 < text.Length)
                {
                    // color tag
                    if (text[i + 1] == 'c' && i + 2 < text.Length && text[i + 2] == '[')
                    {
                        int close = text.IndexOf(']', i + 3);
                        if (close > 0)
                        {
                            FlushWord();
                            string colorStr = text[(i + 3)..close];
                            currentColor = ParseColor(colorStr, defaultColor);
                            i = close + 1;
                            continue;
                        }
                    }
                    // sprite tag
                    else if (text[i + 1] == 's' && i + 2 < text.Length && text[i + 2] == '[')
                    {
                        int close = text.IndexOf(']', i + 3);
                        if (close > 0)
                        {
                            FlushWord();
                            string spriteKey = text[(i + 3)..close];
                            RichSprite s = new RichSprite(spriteKey, RichSpriteRegistry.GetSourceFor(spriteKey), 1f, currentColor);
                            elements.Add(s);
                            i = close + 1;

                            if (text.Length >= close + 2)
                            {
                                if (text[close + 1] == '\\' && text[close + 2] == '!')
                                {
                                    s.Clickable = true;
                                    i += 2;
                                }
                            }
                            continue;
                        }
                    }
                    // link tag \L[url|display] or \L[url]
                    else if (text[i + 1] == 'L' && i + 2 < text.Length && text[i + 2] == '[')
                    {
                        int close = text.IndexOf(']', i + 3);
                        if (close > 0)
                        {
                            FlushWord();
                            string content = text[(i + 3)..close];
                            string linkUrl = content;
                            string displayText = content;

                            int eq = content.IndexOf('|');
                            if (eq >= 0)
                            {
                                linkUrl = content[..eq];
                                displayText = content[(eq + 1)..];
                            }

                            // split displayText into words and spaces, emit RichWord with LinkUrl
                            var parts = displayText.Split(' ');
                            for (int p = 0; p < parts.Length; p++)
                            {
                                if (parts[p].Length > 0)
                                    elements.Add(new RichWord(parts[p], currentColor, linkUrl));
                                if (p < parts.Length - 1)
                                    elements.Add(new RichGlyph(' ', currentColor));
                            }

                            i = close + 1;
                            continue;
                        }
                    }
                }
            }
            catch
            {
                // fall through to treat as normal char
            }

            char c = text[i];
            // whitespace handling: flush word, then add a glyph for the whitespace
            if (char.IsWhiteSpace(c))
            {
                FlushWord();
                elements.Add(new RichGlyph(c, currentColor));
                i++;
                continue;
            }

            // accumulate into current word
            sb.Append(c);
            i++;
        }

        // final flush
        if (sb.Length > 0) FlushWord();

        return new RichText(elements)
        {
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
                case RichWord word:
                    width += Raylib.MeasureTextEx(font.Value, word.Text, FontSize, 1).X;
                    break;

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
            if (element is RichWord word)
            {
                clonedElements.Add(new RichWord(word.Text, word.Color, word.LinkUrl));
            }
            else if (element is RichGlyph glyph)
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
