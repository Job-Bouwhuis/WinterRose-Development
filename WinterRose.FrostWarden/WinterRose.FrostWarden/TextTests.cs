namespace WinterRose.FrostWarden;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;

public static class RichTextRenderer
{
    public class RichChar
    {
        public char Character;
        public Color Color;

        public RichChar(char c, Color color)
        {
            Character = c;
            Color = color;
        }
    }

    public static List<RichChar> ParseRichText(string text, Color defaultColor)
    {
        var result = new List<RichChar>();
        foreach (char c in text)
            result.Add(new RichChar(c, defaultColor));
        return result;
    }

    public static void DrawRichText(List<RichChar> richText, Vector2 position, Font? font, float fontSize, float maxWidth)
    {
        Font actualFont = font ?? Raylib.GetFontDefault();
        var lines = WrapText(richText, actualFont, fontSize, maxWidth);
        float lineSpacing = fontSize * .15f;

        for (int i = 0; i < lines.Count; i++)
        {
            float x = position.X;
            foreach (var rc in lines[i])
            {
                string ch = rc.Character.ToString();
                var size = Raylib.MeasureTextEx(actualFont, ch, fontSize, 1);
                Raylib.DrawTextEx(actualFont, ch, new Vector2(x, position.Y + i * lineSpacing), fontSize, 1, rc.Color);
                x += size.X + lineSpacing;
            }
        }
    }


    private static List<List<RichChar>> WrapText(List<RichChar> richText, Font font, float fontSize, float maxWidth)
    {
        var lines = new List<List<RichChar>>();
        var currentLine = new List<RichChar>();
        var currentWord = new List<RichChar>();
        float currentLineWidth = 0;
        float spaceWidth = Raylib.MeasureTextEx(font, " ", fontSize, 1).X;

        foreach (var rc in richText)
        {
            if (rc.Character == ' ')
            {
                float wordWidth = MeasureWord(currentWord, font, fontSize);
                if (currentLineWidth + wordWidth + spaceWidth > maxWidth)
                {
                    if (currentLine.Count > 0)
                        lines.Add(new List<RichChar>(currentLine));
                    currentLine.Clear();
                    currentLineWidth = 0;
                }

                currentLine.AddRange(currentWord);
                currentLine.Add(new RichChar(' ', rc.Color));
                currentLineWidth += wordWidth + spaceWidth;
                currentWord.Clear();
            }
            else
            {
                currentWord.Add(rc);
            }
        }

        if (currentWord.Count > 0)
        {
            float wordWidth = MeasureWord(currentWord, font, fontSize);
            if (currentLineWidth + wordWidth > maxWidth)
            {
                if (currentLine.Count > 0)
                    lines.Add(new List<RichChar>(currentLine));
                currentLine.Clear();
            }

            currentLine.AddRange(currentWord);
        }

        if (currentLine.Count > 0)
            lines.Add(currentLine);

        return lines;
    }

    private static float MeasureWord(List<RichChar> word, Font font, float fontSize)
    {
        float width = 0;
        foreach (var rc in word)
        {
            var size = Raylib.MeasureTextEx(font, rc.Character.ToString(), fontSize, 1);
            width += size.X;
        }
        return width;
    }
}
