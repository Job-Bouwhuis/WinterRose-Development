namespace WinterRose.FrostWarden.TextRendering;

using Raylib_cs;

public class RichGlyph : RichElement
{
    public char Character;
    public Color Color;

    public RichGlyph(char character, Color color)
    {
        Character = character;
        Color = color;
    }

    public override string ToString() => Character.ToString();
}
