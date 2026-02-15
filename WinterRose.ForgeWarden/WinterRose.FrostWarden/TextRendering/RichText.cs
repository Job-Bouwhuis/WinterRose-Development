namespace WinterRose.ForgeWarden.TextRendering;

using Raylib_cs;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq.Expressions;
using System.Text;
using WinterRose.ForgeWarden.TextRendering.EscapeSequences;
using WinterRose.ForgeWarden.TextRendering.RichElements;

public class RichText
{
    public Font Font { get; set; } = ForgeWardenEngine.DefaultFont;
    public float Spacing { get; set; } = 1;
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

    public int Length
    {
        get
        {
            int totalLength = 0;
            foreach (var e in Elements)
            {
                if (e is RichWord word)
                    totalLength += word.Text.Length;
                else
                    totalLength += 1;
            }
            return totalLength;
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
        // delegate to renderer (reuses WrapText and cached element measurements)
        return RichTextRenderer.MeasureRichText(this, maxWidth);
    }

    internal Rectangle CalculateLineSize(List<RichElement> elements)
    {
        // delegate to renderer to reuse its cached/specialized measurement logic
        return RichTextRenderer.MeasureElements(this, elements);
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
        
        // Initialize modifier stack for lifetime management
        var modifierStack = new ModifierStack();
        modifierStack.RegisterModifier(new BoldModifier());
        modifierStack.RegisterModifier(new ItalicModifier());
        modifierStack.RegisterModifier(new WaveModifier());
        modifierStack.RegisterModifier(new ShakeModifier());
        modifierStack.RegisterModifier(new TypewriterModifier());
        modifierStack.RegisterModifier(new ColorModifier(defaultColor)); // Register default color

        void FlushWord(string? linkUrl = null)
        {
            if (sb.Length == 0) return;
            
            // Check if there's an active link modifier
            if (linkUrl == null && modifierStack.GetSnapshot().StackableModifiers.OfType<LinkModifier>().FirstOrDefault() is LinkModifier linkMod)
            {
                linkUrl = linkMod.Url;
            }
            
            var word = new RichWord(sb.ToString(), currentColor, linkUrl)
            {
                ActiveModifiers = modifierStack.GetSnapshot()
            };
            elements.Add(word);
            sb.Clear();
        }

        int i = 0;
        while (i < text.Length)
        {
            try
            {
                if (text[i] == '\\')
                {
                    int open = text.IndexOf('[', i + 1);
                    if (open > i + 1)
                    {
                        string seq = text[(i + 1)..open];
                        int close = FindMatchingBracket(text, open);
                        if (close > open)
                        {
                            string content = text[(open + 1)..close];

                            // Handle \end[modifierName] to close stackable modifiers
                            if (seq.Equals("end", StringComparison.OrdinalIgnoreCase))
                            {
                                FlushWord();
                                string modifierName = content.Trim().ToLowerInvariant();
                                modifierStack.PopStackable(modifierName);
                                i = close + 1;
                                continue;
                            }

                            // Handle \link[url] to start a link scope
                            if (seq.Equals("link", StringComparison.OrdinalIgnoreCase))
                            {
                                FlushWord();
                                var linkModifier = new LinkModifier(content);
                                modifierStack.RegisterModifier(linkModifier);
                                modifierStack.PushStackable("link");
                                i = close + 1;
                                continue;
                            }

                            // Handle stackable modifiers (bold, italic, wave, shake, tw)
                            switch (seq.ToLowerInvariant())
                            {
                                case "bold":
                                case "b":
                                    FlushWord();
                                    modifierStack.PushStackable("bold");
                                    i = close + 1;
                                    continue;

                                case "italic":
                                case "i":
                                    FlushWord();
                                    modifierStack.PushStackable("italic");
                                    i = close + 1;
                                    continue;

                                case "wave":
                                case "w":
                                    FlushWord();
                                    var waveModifier = new WaveModifier();
                                    // Parse wave parameters if provided
                                    if (!string.IsNullOrEmpty(content))
                                    {
                                        var parts = content.Split(';');
                                        foreach (var part in parts)
                                        {
                                            if (part.Contains('='))
                                            {
                                                var kv = part.Split('=');
                                                var key = kv[0].Trim().ToLowerInvariant();
                                                if (float.TryParse(kv[1].Trim(), out var val))
                                                {
                                                    switch (key)
                                                    {
                                                        case "amplitude": waveModifier.Amplitude = val; break;
                                                        case "speed": waveModifier.Speed = val; break;
                                                        case "wavelength": waveModifier.Wavelength = val; break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    modifierStack.RegisterModifier(waveModifier);
                                    modifierStack.PushStackable("wave");
                                    i = close + 1;
                                    continue;

                                case "shake":
                                case "sh":
                                    FlushWord();
                                    var shakeModifier = new ShakeModifier();
                                    if (!string.IsNullOrEmpty(content))
                                    {
                                        var parts = content.Split(';');
                                        foreach (var part in parts)
                                        {
                                            if (part.Contains('='))
                                            {
                                                var kv = part.Split('=');
                                                var key = kv[0].Trim().ToLowerInvariant();
                                                if (float.TryParse(kv[1].Trim(), out var val))
                                                {
                                                    switch (key)
                                                    {
                                                        case "intensity": shakeModifier.Intensity = val; break;
                                                        case "speed": shakeModifier.Speed = val; break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    modifierStack.RegisterModifier(shakeModifier);
                                    modifierStack.PushStackable("shake");
                                    i = close + 1;
                                    continue;

                                case "typewriter":
                                case "tw":
                                    FlushWord();
                                    var twModifier = new TypewriterModifier();
                                    if (!string.IsNullOrEmpty(content))
                                    {
                                        var parts = content.Split(';');
                                        foreach (var part in parts)
                                        {
                                            if (part.Contains('='))
                                            {
                                                var kv = part.Split('=');
                                                var key = kv[0].Trim().ToLowerInvariant();
                                                if (float.TryParse(kv[1].Trim(), out var val))
                                                {
                                                    if (key == "delay")
                                                        twModifier.CharacterDelay = val;
                                                }
                                            }
                                        }
                                    }
                                    modifierStack.RegisterModifier(twModifier);
                                    modifierStack.PushStackable("typewriter");
                                    i = close + 1;
                                    continue;
                            }

                            // Handle persistent/replacement modifiers via registered handlers
                            var handler = EscapeSequenceHandlerRegistry.GetHandler(seq);
                            if (handler != null)
                            {
                                FlushWord();

                                // For color handler, update currentColor and also track in modifier stack
                                if (handler is ColorSequenceHandler)
                                {
                                    currentColor = ParseColor(content, defaultColor);
                                    var colorModifier = new ColorModifier(currentColor);
                                    modifierStack.RegisterModifier(colorModifier);
                                    modifierStack.SetPersistent("color", colorModifier);
                                    i = close + 1;
                                    continue;
                                }

                                // For other handlers, let them handle element creation
                                int newPos = handler.Parse(content, currentColor, elements, text, close + 1);
                                // Capture modifiers on any created elements
                                for (int j = elements.Count - 1; j >= 0 && elements[j].ActiveModifiers == null; j--)
                                {
                                    elements[j].ActiveModifiers = modifierStack.GetSnapshot();
                                }
                                i = newPos;
                                continue;
                            }

                            // Unknown sequence — treat the backslash as a literal and continue parsing
                            sb.Append('\\');
                            i++;
                            continue;
                        }
                    }

                    // if we didn't find a valid '[...]' sequence, treat '\' as literal
                    sb.Append('\\');
                    i++;
                    continue;
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
                var glyph = new RichGlyph(c, currentColor)
                {
                    ActiveModifiers = modifierStack.GetSnapshot()
                };
                elements.Add(glyph);
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

    static int FindMatchingBracket(string value, int start)
    {
        int depth = 0;

        for (int p = start; p < value.Length; p++)
        {
            char ch = value[p];

            if (ch == '[')
            {
                depth++;
                continue;
            }

            if (ch == ']')
            {
                depth--;

                if (depth == 0)
                    return p;
            }
        }

        return -1;
    }

    public float MeasureTextWidth(Font? font)
    {
        font ??= ForgeWardenEngine.DefaultFont;
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
        if (string.IsNullOrWhiteSpace(input))
            return fallback;

        // Try hex color first (#RRGGBB or #RRGGBBAA)
        if (input.StartsWith("#"))
        {
            if (input.Length == 7) // #RRGGBB
            {
                try
                {
                    return new Color(
                        Convert.ToInt32(input.Substring(1, 2), 16),
                        Convert.ToInt32(input.Substring(3, 2), 16),
                        Convert.ToInt32(input.Substring(5, 2), 16),
                        255
                    );
                }
                catch
                {
                    return fallback;
                }
            }
            else if (input.Length == 9) // #RRGGBBAA
            {
                try
                {
                    return new Color(
                        Convert.ToInt32(input.Substring(1, 2), 16),
                        Convert.ToInt32(input.Substring(3, 2), 16),
                        Convert.ToInt32(input.Substring(5, 2), 16),
                        Convert.ToInt32(input.Substring(7, 2), 16)
                    );
                }
                catch
                {
                    return fallback;
                }
            }
        }

        // Try named color from registry
        if (ColorRegistry.TryGetColor(input, out var namedColor))
            return namedColor;

        // Fallback if not found
        return fallback;
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

    internal bool IsOnlyWhitespace()
    {
        foreach(var e in Elements)
        {
            if (e is RichWord word)
            {
                if (!string.IsNullOrWhiteSpace(word.Text))
                    return false;
            }
            else if (e is RichGlyph glyph)
            {
                if (!char.IsWhiteSpace(glyph.Character))
                    return false;
            }
            else
            {
                return false;
            }
        }
        return true; // if we got here, either there are no elements, or all are whitespace
    }
}
