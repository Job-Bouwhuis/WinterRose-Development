namespace WinterRose.ForgeWarden.TextRendering;

using Raylib_cs;

public class RichGlyph : RichElement
{
    public char Character;
    public Color Color;

    /// <summary>
    /// Set whenever this glyph is part of a link URL sentence
    /// </summary>
    public string? GlyphLinkUrl;

    public RichGlyph(char character, Color color)
    {
        Character = character;
        Color = color;
    }

    public override string ToString() => Character.ToString();
}
