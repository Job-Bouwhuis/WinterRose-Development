namespace WinterRose.FrostWarden.TextRendering;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;

public static class RichTextRenderer
{
    public static void DrawRichText(string text, Vector2 position, Font? font, float fontSize, float maxWidth)
        => DrawRichText(RichText.Parse(text, Color.White), position, font, fontSize, maxWidth);

    public static void DrawRichText(RichText richText, Vector2 position, Font? font, float fontSize, float maxWidth)
        => DrawRichText(richText, position, font, fontSize, maxWidth, Color.White);

    public static void DrawRichText(RichText richText, Vector2 position, Font? font, float fontSize, float maxWidth, Color overallTint)
    {
        Font actualFont = font ?? Raylib.GetFontDefault();
        var lines = WrapText(richText.Elements, actualFont, fontSize, maxWidth);
        float lineSpacing = fontSize * 0.15f;

        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            float x = position.X;
            float y = position.Y + lineIndex * (fontSize + lineSpacing);

            foreach (var element in lines[lineIndex])
            {
                switch (element)
                {
                    case RichGlyph glyph:
                        string ch = glyph.Character.ToString();
                        var glyphSize = Raylib.MeasureTextEx(actualFont, ch, fontSize, 1);

                        // Multiply glyph color with overall tint, clamping per channel
                        Color tintedColor = new Color(
                            (byte)(glyph.Color.R * overallTint.R / 255),
                            (byte)(glyph.Color.G * overallTint.G / 255),
                            (byte)(glyph.Color.B * overallTint.B / 255),
                            (byte)(glyph.Color.A * overallTint.A / 255)
                        );

                        Raylib.DrawTextEx(actualFont, ch, new Vector2(x, y), fontSize, 1, tintedColor);
                        x += glyphSize.X + lineSpacing;
                        break;

                    case RichSprite sprite:
                        var texture = RichSpriteRegistry.GetSprite(sprite.SpriteKey);
                        if (texture is not null)
                        {
                            float spriteHeight = sprite.BaseSize * fontSize;
                            float scale = spriteHeight / texture.Height;

                            // Multiply sprite tint with overall tint the same way
                            Color tintedSpriteColor = new Color(
                                (byte)(sprite.Tint.R * overallTint.R / 255),
                                (byte)(sprite.Tint.G * overallTint.G / 255),
                                (byte)(sprite.Tint.B * overallTint.B / 255),
                                (byte)(sprite.Tint.A * overallTint.A / 255)
                            );

                            Raylib.DrawTextureEx(texture, new Vector2(x, y), 0, scale, tintedSpriteColor);
                            if(sprite.Clickable)
                            {
                                Rectangle imageRect = new Rectangle(
                                    (int)x,
                                    (int)y,
                                    (int)(texture.Width * scale),
                                    (int)(texture.Height * scale));

                                if(ray.CheckCollisionPointRec(ray.GetMousePosition(), imageRect) && ray.IsMouseButtonPressed(MouseButton.Left))
                                {
                                    Console.WriteLine("Sprite Clicked!");

                                    DialogBox.Show("Image", sprite.SpriteSource, DialogType.Sprite, placement: DialogPlacement.CenterBig, priority: DialogPriority.AlwaysFirst);
                                }
                            }

                            x += texture.Width * scale + lineSpacing;
                        }
                        break;
                }
            }
        }
    }




    private static List<List<RichElement>> WrapText(List<RichElement> elements, Font font, float fontSize, float maxWidth)
    {
        var lines = new List<List<RichElement>>();
        var currentLine = new List<RichElement>();
        var currentWord = new List<RichElement>();
        float currentLineWidth = 0;
        float spaceWidth = Raylib.MeasureTextEx(font, " ", fontSize, 1).X;

        foreach (var element in elements)
        {
            bool isSpace = element is RichGlyph g && g.Character == ' ';

            if (isSpace)
            {
                float wordWidth = MeasureWord(currentWord, font, fontSize);
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
                currentWord.Add(element);
            }
        }

        if (currentWord.Count > 0)
        {
            float wordWidth = MeasureWord(currentWord, font, fontSize);
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

    public static float MeasureWord(List<RichElement> word, Font font, float fontSize)
    {
        float width = 0;
        foreach (var element in word)
        {
            switch (element)
            {
                case RichGlyph glyph:
                    width += Raylib.MeasureTextEx(font, glyph.Character.ToString(), fontSize, 1).X;
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
}
