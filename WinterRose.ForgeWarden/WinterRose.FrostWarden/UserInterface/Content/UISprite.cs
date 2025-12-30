using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.UserInterface;

namespace WinterRose.ForgeWarden.UserInterface;
public class UISprite : UIContent
{
    public Sprite? Sprite { get; set; }

    public float? MaxWidth { get; set; }   // optional maximum width
    public float? MaxHeight { get; set; }  // optional maximum height

    public UISprite(string spriteSource) => Sprite = SpriteCache.Get(spriteSource);

    public UISprite(Sprite sprite) => Sprite = sprite;

    public UISprite()
    {
    }

    protected internal override float GetHeight(float width)
    {
        if (Sprite == null) return 0f;

        float aspect = (float)Sprite.Width / Sprite.Height;
        float targetWidth = width;

        // apply max width if specified
        if (MaxWidth.HasValue)
            targetWidth = Math.Min(targetWidth, MaxWidth.Value);

        float targetHeight = targetWidth / aspect;

        // apply max height if specified
        if (MaxHeight.HasValue)
            targetHeight = Math.Min(targetHeight, MaxHeight.Value);

        return targetHeight;
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        float width = availableArea.Width;

        if (MaxWidth.HasValue)
            width = Math.Min(width, MaxWidth.Value);

        float height = GetHeight(width);

        if (MaxHeight.HasValue)
            height = Math.Min(height, MaxHeight.Value);

        return new Vector2(width, height);
    }

    public void ForceDraw(Rectangle bounds) => Draw(bounds);

    protected override void Draw(Rectangle bounds)
    {
        if (Sprite == null) return;

        float aspect = (float)Sprite.Width / Sprite.Height;

        float targetWidth = bounds.Width;
        float targetHeight = targetWidth / aspect;

        // apply max constraints if set
        if (MaxWidth.HasValue)
            targetWidth = Math.Min(targetWidth, MaxWidth.Value);
        if (MaxHeight.HasValue && targetHeight > MaxHeight.Value)
        {
            targetHeight = MaxHeight.Value;
            targetWidth = targetHeight * aspect;
        }
        else if (targetHeight > bounds.Height)
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


