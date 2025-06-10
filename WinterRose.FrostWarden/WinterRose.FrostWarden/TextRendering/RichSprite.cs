namespace WinterRose.FrostWarden.TextRendering;

using Raylib_cs;

public class RichSprite : RichElement
{
    public string SpriteKey;
    public float BaseSize;
    public Color Tint;

    public RichSprite(string spriteKey, float baseSize, Color tint)
    {
        SpriteKey = spriteKey;
        BaseSize = baseSize;
        Tint = tint;
    }
}
