namespace WinterRose.ForgeWarden.TextRendering;

using Raylib_cs;

public class RichSprite : RichElement
{
    public string SpriteKey { get; set; }
    public string SpriteSource { get; set; }
    public float BaseSize { get; set; }
    public Color Tint { get; set; }
    public bool Clickable { get; internal set; }

    public RichSprite(string spriteKey, string source, float baseSize, Color tint)
    {
        SpriteKey = spriteKey;
        SpriteSource = source;
        BaseSize = baseSize;
        Tint = tint;
    }

    public override string ToString() => $"\\s[{SpriteKey}]";
}
