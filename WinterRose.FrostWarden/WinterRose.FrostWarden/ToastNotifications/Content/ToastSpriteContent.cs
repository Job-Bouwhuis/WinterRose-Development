using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.ToastNotifications;
public class ToastSpriteContent : ToastContent
{
    public string SpriteSource { get; private set; }
    private Sprite? sprite;

    public ToastSpriteContent(string spriteSource)
    {
        SpriteSource = spriteSource;
        sprite = SpriteCache.Get(spriteSource);
    }

    public ToastSpriteContent(Sprite sprite)
    {
        SpriteSource = sprite.Source;
        this.sprite = sprite;
    }

    public override float GetHeight(float width)
    {
        if (sprite == null) return 0f;

        float aspect = (float)sprite.Width / sprite.Height;
        float targetWidth = width;
        float targetHeight = targetWidth / aspect;

        return targetHeight;
    }

    public override void Draw(Rectangle bounds, float contentAlpha)
    {
        if (sprite == null) return;

        float aspect = (float)sprite.Width / sprite.Height;

        float targetWidth = bounds.Width;
        float targetHeight = targetWidth / aspect;

        if (targetHeight > bounds.Height)
        {
            targetHeight = bounds.Height;
            targetWidth = targetHeight * aspect;
        }

        float drawX = bounds.X + (bounds.Width - targetWidth) / 2;
        float drawY = bounds.Y + (bounds.Height - targetHeight) / 2;

        Color tint = new Color(
            Style.ContentColor.R,
            Style.ContentColor.G,
            Style.ContentColor.B,
            (int)(Style.ContentColor.A * contentAlpha)
        );

        ray.DrawTexturePro(
            sprite.Texture,
            new Rectangle(0, 0, sprite.Width, sprite.Height),
            new Rectangle(drawX, drawY, targetWidth, targetHeight),
            new Vector2(0, 0),
            0f,
            tint
        );
    }

    public override void OnClick(MouseButton button)
    {
        // could extend later to open image, zoom, etc.
    }

    public override void OnHover()
    {
        // could extend later to show tooltip or preview
    }
}

