using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.UserInterface;

namespace WinterRose.ForgeWarden.UserInterface.Content;
public class ToastSpriteContent : UIContent
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

    protected internal override float GetHeight(float width)
    {
        if (sprite == null) return 0f;

        float aspect = (float)sprite.Width / sprite.Height;
        float targetWidth = width;
        float targetHeight = targetWidth / aspect;

        return targetHeight;
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        return new(availableArea.Width, GetHeight(availableArea.Width));
    }

    protected internal override void Draw(Rectangle bounds)
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

        Color tint = Style.ContentTint;

        ray.DrawTexturePro(
            sprite.Texture,
            new Rectangle(0, 0, sprite.Width, sprite.Height),
            new Rectangle(drawX, drawY, targetWidth, targetHeight),
            new Vector2(0, 0),
            0f,
            tint
        );
    }
}

