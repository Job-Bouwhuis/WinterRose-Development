using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.Utility;
using WinterRose.Recordium;

namespace WinterRose.ForgeWarden.UserInterface.Content;

public class AsyncImageContent : UIContent
{
    private UISprite sprite = null!;
    private UIProgress spinner;
    private bool loaded;
    private readonly string url;

    private readonly float? maxWidth;
    private readonly float? maxHeight;

    public AsyncImageContent(string imageUrl, float? maxWidth = null, float? maxHeight = null)
    {
        url = imageUrl;
        this.maxWidth = maxWidth;
        this.maxHeight = maxHeight;
        spinner = new UIProgress(-1, infiniteSpinText: "Fetching image...");
        _ = LoadAndSwapAsync();
    }

    protected internal override void Setup()
    {
        spinner.Owner = Owner;
        spinner.Setup();
    }

    private async Task LoadAndSwapAsync()
    {
        try
        {
            var sprite = await HttpImageLoader.LoadSpriteFromUrlAsync(url);
            if (sprite != null)
            {
                if (sprite.Texture.Id == 0)
                {
                    UIText t = new("\\c[red]Image failed to load.");
                    Owner.AddContent(this, t, Owner.GetContentIndex(this));
                    Owner.RemoveContent(this);
                    return;
                }
                this.sprite= new UISprite(sprite);
                this.sprite.MaxWidth = maxWidth;
                this.sprite.MaxHeight = maxHeight;
                if(Owner is null)
                    await WinterUtils.AwaitNotNull(() => Owner);

                Owner.AddContent(this, this.sprite, Owner.GetContentIndex(this));
                Owner.RemoveContent(this);
            }
        }
        catch (Exception e)
        {
            // swallow - we'll leave spinner gone and show empty spacer
        }
        finally
        {
            loaded = true;
        }
    }

    protected internal override float GetHeight(float width)
    {
        if (!loaded) return spinner.GetHeight(width);
        if (sprite != null) return sprite.GetHeight(width);
        return 0f;
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        if (!loaded) return spinner.GetSize(availableArea);
        if (sprite != null) return sprite.GetSize(availableArea);
        return new Vector2(availableArea.Width, 0);
    }

    protected override void Update()
    {
        if (!loaded)
            spinner._Update();
    }

    protected override void Draw(Rectangle bounds)
    {
        if (!loaded)
        {
            spinner.ForceDraw(bounds);
            return;
        }
        if (sprite != null)
        {
            sprite.ForceDraw(bounds);
            return;
        }

        // nothing loaded -> optionally draw a subtle placeholder / nothing
    }
}