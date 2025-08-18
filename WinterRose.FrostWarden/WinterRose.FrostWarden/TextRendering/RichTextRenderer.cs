namespace WinterRose.ForgeWarden.TextRendering;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using WinterRose.ForgeWarden.DialogBoxes;
using WinterRose.ForgeWarden.DialogBoxes.Boxes;

public static class RichTextRenderer
{
    public static void DrawRichText(string text, Vector2 position, float maxWidth)
        => DrawRichText(RichText.Parse(text, Color.White), position, maxWidth);

    public static void DrawRichText(RichText richText, Vector2 position, float maxWidth)
        => DrawRichText(richText, position, maxWidth, Color.White);

    public static void DrawRichText(RichText richText, Vector2 position, float maxWidth, Color overallTint)
    {
        var lines = WrapText(richText, maxWidth);

        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            float x = position.X;
            float y = position.Y + lineIndex * (richText.FontSize + richText.Spacing);

            foreach (var element in lines[lineIndex])
            {
                switch (element)
                {
                    case RichGlyph glyph:
                        string ch = glyph.Character.ToString();
                        var glyphSize = Raylib.MeasureTextEx(richText.Font, ch, richText.FontSize, richText.Spacing);

                        Color tintedColor = new(
                            (byte)(glyph.Color.R * overallTint.R / 255),
                            (byte)(glyph.Color.G * overallTint.G / 255),
                            (byte)(glyph.Color.B * overallTint.B / 255),
                            (byte)(glyph.Color.A * overallTint.A / 255)
                        );

                        Raylib.DrawTextEx(richText.Font, ch, new Vector2(x, y), richText.FontSize, 1, tintedColor);
                        x += glyphSize.X + richText.Spacing;
                        break;

                    case RichSprite sprite:
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
                                Rectangle imageRect = new Rectangle(
                                    (int)x,
                                    (int)y,
                                    (int)(texture.Width * scale),
                                    (int)(texture.Height * scale));

                                if (ray.CheckCollisionPointRec(ray.GetMousePosition(), imageRect) && ray.IsMouseButtonPressed(MouseButton.Left))
                                    Dialogs.Show(new SpriteDialog(
                                        "Image",
                                        sprite.SpriteSource,
                                        DialogPlacement.CenterBig,
                                        DialogPriority.EngineNotifications));
                            }

                            x += texture.Width * scale + richText.Spacing;
                        }
                        break;
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
                float wordWidth = text.CalculateLineSize(currentWord).X;
                if (currentLineWidth + wordWidth + spaceWidth > maxWidth)
                {
                    if (currentLine.Count > 0)
                        lines.Add([.. currentLine]);
                    currentLine.Clear();
                    currentLineWidth = 0;
                }

                currentLine.AddRange(currentWord);
                currentLine.Add(element); // the space itself
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
        }

        if (currentWord.Count > 0)
        {
            float wordWidth = text.CalculateLineSize(currentWord).X;
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
}
