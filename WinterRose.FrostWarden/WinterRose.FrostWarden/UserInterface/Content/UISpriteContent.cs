using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.UserInterface;

namespace WinterRose.ForgeWarden.UserInterface;
public class UISpriteContent : UIContent
{
    public Sprite? Sprite { get; set; }

    public UISpriteContent(string spriteSource) => Sprite = SpriteCache.Get(spriteSource);

    public UISpriteContent(Sprite sprite) => Sprite = sprite;

    public UISpriteContent()
    {

    }

    protected internal override float GetHeight(float width)
    {
        if (Sprite == null) return 0f;

        float aspect = (float)Sprite.Width / Sprite.Height;
        float targetWidth = width;
        float targetHeight = targetWidth / aspect;

        return targetHeight;
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        return new(availableArea.Width, GetHeight(availableArea.Width));
    }

    protected override void Draw(Rectangle bounds)
    {
        if (Sprite == null) return;

        float aspect = (float)Sprite.Width / Sprite.Height;

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
            Sprite.Texture,
            new Rectangle(0, 0, Sprite.Width, Sprite.Height),
            new Rectangle(drawX, drawY, targetWidth, targetHeight),
            new Vector2(0, 0),
            0f,
            tint
        );
    }
}

