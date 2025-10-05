namespace WinterRose.ForgeWarden.TextRendering;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;

public static class RichTextRenderer
{
    private static readonly Dictionary<string, Vector2> MEASURE_CACHE = new();
    private static readonly Dictionary<string, Vector2> SPRITE_SIZE_CACHE = new();

    public static void DrawRichText(string text, Vector2 position, float maxWidth, InputContext input)
        => DrawRichText(RichText.Parse(text, Color.White), position, maxWidth, input);

    public static void DrawRichText(RichText richText, Vector2 position, float maxWidth, InputContext input)
        => DrawRichText(richText, position, maxWidth, Color.White, input);

    private static string GetMeasureKey(Font font, string s, float fontSize, float spacing)
    => $"{font.GetHashCode()}|{fontSize}|{spacing}|{s}";

    private static Vector2 MeasureTextExCached(Font font, string s, float fontSize, float spacing)
    {
        string key = GetMeasureKey(font, s, fontSize, spacing);
        if (MEASURE_CACHE.TryGetValue(key, out var cached))
            return cached;

        var measured = Raylib.MeasureTextEx(font, s, fontSize, spacing);
        MEASURE_CACHE[key] = measured;
        return measured;
    }

    public static void DrawRichText(RichText richText, Vector2 position, float maxWidth, Color overallTint, InputContext input)
    {
        var lines = WrapText(richText, maxWidth);

        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            float x = position.X;
            float y = position.Y + lineIndex * (richText.FontSize + richText.Spacing);

            // collect link hitboxes for this line and process clicks after drawing
            var linkHitboxes = new List<(string Url, Rectangle Rect, Color Tint)>();
            var line = lines[lineIndex];

            int i = 0;
            while (i < line.Count)
            {
                // handle glyph runs
                if (line[i] is RichGlyph)
                {
                    var runSb = new StringBuilder();
                    Color runTint = default;
                    string runUrl = null;
                    bool runTintInit = false;

                    int j = i;
                    for (; j < line.Count; j++)
                    {
                        if (line[j] is not RichGlyph g) break;

                        // compute tinted color for this glyph
                        Color glyphTint = new(
                            (byte)(g.Color.R * overallTint.R / 255),
                            (byte)(g.Color.G * overallTint.G / 255),
                            (byte)(g.Color.B * overallTint.B / 255),
                            (byte)(g.Color.A * overallTint.A / 255)
                        );

                        if (!runTintInit)
                        {
                            runTint = glyphTint;
                            runUrl = g.GlyphLinkUrl;
                            runTintInit = true;
                        }
                        else
                        {
                            // break run if tint or url differ
                            if (glyphTint.R != runTint.R || glyphTint.G != runTint.G || glyphTint.B != runTint.B || glyphTint.A != runTint.A
                                || (g.GlyphLinkUrl != runUrl))
                            {
                                break;
                            }
                        }

                        runSb.Append(g.Character);
                    }

                    string runText = runSb.ToString();
                    if (runText.Length > 0)
                    {
                        var runSize = MeasureTextExCached(richText.Font, runText, richText.FontSize, richText.Spacing);
                        Raylib.DrawTextEx(richText.Font, runText, new Vector2(x, y), richText.FontSize, richText.Spacing, runTint);

                        if (runUrl is not null)
                        {
                            // underline whole run
                            Raylib.DrawLineEx(new Vector2(x, y + runSize.Y + 2), new Vector2(x + runSize.X, y + runSize.Y + 2), 1, runTint);
                            linkHitboxes.Add((runUrl, new Rectangle((int)x, (int)y, (int)runSize.X, (int)runSize.Y), runTint));
                        }

                        x += runSize.X + richText.Spacing;
                    }

                    i = j;
                    continue;
                }

                // handle sprites
                if (line[i] is RichSprite sprite)
                {
                    var texture = RichSpriteRegistry.GetSprite(sprite.SpriteKey);
                    if (texture is not null)
                    {
                        float spriteHeight = sprite.BaseSize * richText.FontSize;
                        float scale = spriteHeight / texture.Height;

                        Color tintedSpriteColor = new Color(
                            (byte)(sprite.Tint.R * overallTint.R / 255),
                            (byte)(sprite.Tint.G * overallTint.G / 255),
                            (byte)(sprite.Tint.B * overallTint.B / 255),
                            (byte)(sprite.Tint.A * overallTint.A / 255)
                        );

                        Raylib.DrawTextureEx(texture, new Vector2(x, y), 0, scale, tintedSpriteColor);

                        if (sprite.Clickable)
                        {
                            var imageRect = new Rectangle((int)x, (int)y, (int)(texture.Width * scale), (int)(texture.Height * scale));
                            // store a temporary "fake" url with null to reuse link processing; handle directly
                            if (ray.CheckCollisionPointRec(input.MousePosition, imageRect) && ray.IsMouseButtonPressed(MouseButton.Left))
                            {
                                Toasts.Error("Sprite dialog is temporarily out of order");
                            }
                        }

                        x += texture.Width * scale + richText.Spacing;
                    }

                    i++;
                    continue;
                }

                if (line[i] is RichWord word)
                {
                    Color tinted = new Color(
                        (byte)(word.Color.R * overallTint.R / 255),
                        (byte)(word.Color.G * overallTint.G / 255),
                        (byte)(word.Color.B * overallTint.B / 255),
                        (byte)(word.Color.A * overallTint.A / 255)
                    );

                    var runSize = MeasureTextExCached(richText.Font, word.Text, richText.FontSize, richText.Spacing);
                    Raylib.DrawTextEx(richText.Font, word.Text, new Vector2(x, y), richText.FontSize, richText.Spacing, tinted);

                    if (!string.IsNullOrEmpty(word.LinkUrl))
                    {
                        Raylib.DrawLineEx(new Vector2(x, y + runSize.Y + 2), new Vector2(x + runSize.X, y + runSize.Y + 2), 1, tinted);
                        linkHitboxes.Add((word.LinkUrl, new Rectangle((int)x, (int)y, (int)runSize.X, (int)runSize.Y), tinted));
                    }

                    x += runSize.X + richText.Spacing;
                    i++;
                    continue;
                }

                i++;
            }

            // process link clicks for this line (one check per run)
            foreach (var (Url, Rect, Tint) in linkHitboxes)
            {
                if (Url is not null && ray.CheckCollisionPointRec(input.MousePosition, Rect) && ray.IsMouseButtonPressed(MouseButton.Left))
                {
                    Dialogs.Show(new BrowserDialog(Url, DialogPlacement.CenterBig, DialogPriority.EngineNotifications));
                }
            }
        }
    }
    internal static List<List<RichElement>> WrapText(RichText text, float maxWidth)
    {
        var elements = text.Elements;
        var lines = new List<List<RichElement>>();
        var currentLine = new List<RichElement>();
        var currentWord = new List<RichElement>();
        float currentLineWidth = 0;
        float spaceWidth = Raylib.MeasureTextEx(text.Font, " ", text.FontSize, text.Spacing).X;

        foreach (var element in elements)
        {
            bool isSpace = element is RichGlyph g && g.Character == ' ';

            if (isSpace)
            {
                float wordWidth = text.CalculateLineSize(currentWord).Width;
                if (currentLineWidth + wordWidth + spaceWidth > maxWidth)
                {
                    if (currentLine.Count > 0)
                        lines.Add([.. currentLine]);
                    currentLine.Clear();
                    currentLineWidth = 0;
                }

                currentLine.AddRange(currentWord);
                currentLine.Add(element);
                currentLineWidth += wordWidth + spaceWidth;
                currentWord.Clear();
            }
            else
            {
                if (element is RichGlyph gg && gg.Character == '\n')
                {
                    float wordWidth = text.CalculateLineSize(currentWord).X;
                    currentLine.AddRange(currentWord);
                    currentLine.Add(element); // the newline itself
                    currentLineWidth += wordWidth + spaceWidth;
                    currentWord.Clear();

                    if (currentLine.Count > 0)
                        lines.Add([.. currentLine]);
                    currentLine.Clear();
                    continue;
                }
                currentWord.Add(element);
            }

            if (currentWord.Count > 0)
            {
                float wordWidth = text.CalculateLineSize(currentWord).Width;
                if (currentLineWidth + wordWidth > maxWidth)
                {
                    if (currentLine.Count > 0)
                        lines.Add([.. currentLine]);
                    currentLine.Clear();
                    currentLineWidth = 0;
                }
            }
        }

        if (currentWord.Count > 0)
        {
            float wordWidth = text.CalculateLineSize(currentWord).Width;
            if (currentLineWidth + wordWidth > maxWidth)
            {
                if (currentLine.Count > 0)
                    lines.Add([.. currentLine]);
                currentLine.Clear();
            }

            currentLine.AddRange(currentWord);
        }

        if (currentLine.Count > 0)
            lines.Add(currentLine);

        return lines;
    }

    private static Vector2 CalculateElementsSize(RichText text, List<RichElement> elements)
    {
        // sums width of a run of glyphs and sprites; returns combined width & max height
        var sb = new StringBuilder();
        float width = 0f;
        float maxHeight = 0f;

        void FlushStringBuilder()
        {
            if (sb.Length == 0) return;
            var m = MeasureTextExCached(text.Font, sb.ToString(), text.FontSize, text.Spacing);
            width += m.X;
            if (m.Y > maxHeight) maxHeight = m.Y;
            sb.Clear();
        }

        foreach (var element in elements)
        {
            switch (element)
            {
                case RichGlyph g:
                    sb.Append(g.Character);
                    break;

                case RichSprite s:
                    FlushStringBuilder();
                    // cache per sprite key + fontSize because scale depends on fontSize
                    string key = s.SpriteKey + "|" + text.FontSize;
                    if (!SPRITE_SIZE_CACHE.TryGetValue(key, out var size))
                    {
                        var texture = RichSpriteRegistry.GetSprite(s.SpriteKey);
                        if (texture is not null)
                        {
                            float spriteHeight = s.BaseSize * text.FontSize;
                            float scale = spriteHeight / texture.Height;
                            size = new Vector2(texture.Width * scale, texture.Height * scale);
                        }
                        else
                        {
                            size = Vector2.Zero;
                        }
                        SPRITE_SIZE_CACHE[key] = size;
                    }
                    width += size.X;
                    if (size.Y > maxHeight) maxHeight = size.Y;
                    break;
            }
        }

        FlushStringBuilder();
        return new Vector2(width, maxHeight);
    }
}
