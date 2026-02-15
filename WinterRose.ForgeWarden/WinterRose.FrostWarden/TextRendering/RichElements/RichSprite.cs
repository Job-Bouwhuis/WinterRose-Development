namespace WinterRose.ForgeWarden.TextRendering.RichElements;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;

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

    public override RichTextRenderResult Render(RichTextRenderContext context, Vector2 position)
    {
        float x = position.X;
        float y = position.Y;

        var texture = RichSpriteRegistry.GetSprite(SpriteKey);
        if (texture is null)
        {
            return new RichTextRenderResult { WidthConsumed = 0, HeightConsumed = 0 };
        }

        float spriteHeight = BaseSize * context.RichText.FontSize;
        float scale = spriteHeight / texture.Height;

        Color tintedSpriteColor = new Color(
            (byte)(Tint.R * context.OverallTint.R / 255),
            (byte)(Tint.G * context.OverallTint.G / 255),
            (byte)(Tint.B * context.OverallTint.B / 255),
            (byte)(Tint.A * context.OverallTint.A / 255)
        );

        Raylib.DrawTextureEx(texture, new Vector2(x, y), 0, scale, tintedSpriteColor);

        if (Clickable && context.Input != null)
        {
            var imageRect = new Rectangle((int)x, (int)y, (int)(texture.Width * scale), (int)(texture.Height * scale));
            
            if (Raylib.CheckCollisionPointRec(context.Input.MousePosition, imageRect) && context.Input.IsDown(MouseButton.Left))
            {
                Toasts.Error("Sprite dialog is temporarily out of order");
            }
        }

        return new RichTextRenderResult
        {
            WidthConsumed = texture.Width * scale + context.RichText.Spacing,
            HeightConsumed = texture.Height * scale
        };
    }

    public override float MeasureWidth(RichText richText, Dictionary<string, Vector2> measureCache)
    {
        string key = SpriteKey + "|" + richText.FontSize;
        if (!measureCache.TryGetValue(key, out var size))
        {
            var texture = RichSpriteRegistry.GetSprite(SpriteKey);
            if (texture is not null)
            {
                float spriteHeight = BaseSize * richText.FontSize;
                float scale = spriteHeight / texture.Height;
                size = new Vector2(texture.Width * scale, texture.Height * scale);
            }
            else
            {
                size = Vector2.Zero;
            }
            measureCache[key] = size;
        }

        return size.X;
    }
}
