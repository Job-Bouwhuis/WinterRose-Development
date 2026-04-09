using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.Input;

namespace WinterRose.ForgeWarden.UserInterface.Content;

/// <summary>
/// Viewport content that displays a RenderTexture2D in the UI
/// </summary>
public class UIViewport : UIContent
{
    private RenderTexture2D renderTexture;
    public Vector2 ViewportSize { get; set; } = new Vector2(800, 600);

    public UIViewport(RenderTexture2D renderTexture)
    {
        this.renderTexture = renderTexture;
    }

    public void SetRenderTexture(RenderTexture2D texture)
    {
        renderTexture = texture;
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        return new Vector2(
            Math.Min(ViewportSize.X, availableArea.Width),
            Math.Min(ViewportSize.Y, availableArea.Height)
        );
    }

    protected override void Draw(Rectangle bounds)
    {
        if (renderTexture.Id == 0)
            return;

        Raylib.DrawTexturePro(
            renderTexture.Texture,
            new Rectangle(0, 0, renderTexture.Texture.Width, -renderTexture.Texture.Height),
            bounds,
            Vector2.Zero,
            0,
            Color.White);

        Raylib.DrawRectangleLines(
            (int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height,
            Color.DarkGray);
    }

    internal protected override float GetHeight(float maxWidth)
    {
        return ViewportSize.Y;
    }
}
