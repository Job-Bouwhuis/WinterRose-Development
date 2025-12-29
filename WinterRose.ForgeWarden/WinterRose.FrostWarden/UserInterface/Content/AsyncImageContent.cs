using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.Utility;
using WinterRose.Recordium;

namespace WinterRose.ForgeWarden.UserInterface.Content;

public class AsyncImageContent : UIContent
{
    private UISpriteContent spriteContent = null!;
    private UIProgress spinner;
    private bool loaded;
    private readonly string url;

    public AsyncImageContent(string imageUrl)
    {
        url = imageUrl;
        spinner = new UIProgress(-1, infiniteSpinText: "Fetching image...");
        _ = LoadAndSwapAsync();
    }

    protected internal override void Setup()
    {
        spinner.owner = owner;
        spinner.Setup();
    }

    private async Task LoadAndSwapAsync()
    {
        try
        {
            var sprite = await HtmlImageLoader.LoadSpriteFromUrlAsync(url);
            if (sprite != null)
            {
                spriteContent = new UISpriteContent(sprite);
                spriteContent.owner = owner;
                spriteContent.Setup();
            }
        }
        catch
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
        if (spriteContent != null) return spriteContent.GetHeight(width);
        return 0f;
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        if (!loaded) return spinner.GetSize(availableArea);
        if (spriteContent != null) return spriteContent.GetSize(availableArea);
        return new Vector2(availableArea.Width, 0);
    }

    protected internal override void Update()
    {
        if (!loaded)
            spinner.Update();
    }

    protected override void Draw(Rectangle bounds)
    {
        if (!loaded)
        {
            spinner.ForceDraw(bounds);
            return;
        }
        if (spriteContent != null)
        {
            spriteContent.ForceDraw(bounds);
            return;
        }

        // nothing loaded -> optionally draw a subtle placeholder / nothing
    }
}