using Microsoft.VisualBasic.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D9;
using System;

namespace WinterRose.Monogame;

/// <summary>
/// A static sprite renderer implementing <see cref="Renderer"/>
/// </summary>
public sealed class SpriteRenderer : Renderer
{
    /// <summary>
    /// Gets the sprite selected for this SpriteRenderer
    /// </summary>
    [IncludeWithSerialization]
    public Sprite Sprite { get => tex; set => tex = value; }

    /// <summary>
    /// When not null, only this rectangle is drawn by this sprite renderer
    /// </summary>
    [IncludeWithSerialization]
    public Rectangle? ClipMask { get; set; } = null;

    /// <summary>
    /// The draw origin of this sprite, values between 0 and 1 are considered inside the bounds of the sprite. values outside 0 and 1 are accepted
    /// </summary>
    [IncludeWithSerialization]
    public Vector2 Origin
    {
        get => origin;
        set
        {
            if (value.X is > 1 or < 0 || value.Y is > 1 or < 0)
                throw new("Origin must be between 0 and 1");
            origin = value;
        }
    }

    /// <summary>
    /// The time it took to render the sprite
    /// </summary>
    public override TimeSpan DrawTime { get; protected set; }

    /// <summary>
    /// The bounds of the texture this <see cref="SpriteRenderer"/> renders
    /// </summary>
    public override RectangleF Bounds
    {
        get
        {
            if (tex is null)
            {
                RectangleF r = new RectangleF();
                r.Position = transform.position;
                r.Size = new(0, 0);
                return r;
            }
            RectangleF rect = ((RectangleF?)tex.Bounds) ?? RectangleF.Zero;
            rect.Position = (transform.position - GetTrueOrigin());
            return rect;
        }
    }
    /// <summary>
    /// The <see cref="SpriteEffects"/> used when rendering the sprite
    /// </summary>
    [IncludeWithSerialization]
    public SpriteEffects Effects { get; set; } = SpriteEffects.None;
    /// <summary>
    /// Layerdepth, used to determain what render element should be drawn in front of another
    /// </summary>
    [IncludeInTemplateCreation]
    [IncludeWithSerialization]
    public float LayerDepth { get; set; } = 0.5f;
    /// <summary>
    /// The tint color of the sprite
    /// </summary>
    [IncludeWithSerialization]
    public Color Tint { get; set; } = Color.White;

    private Sprite tex;
    private Vector2 origin = new(0.5f, 0.5f);

    /// <summary>
    /// Creates a new empty <see cref="SpriteRenderer"/>
    /// </summary>
    public SpriteRenderer()
    {
        Sprite = new(1, 1, new Color(0, 0, 0, 0));
    }
    /// <summary>
    /// Creates a new <see cref="SpriteRenderer"/> with <paramref name="tex"/> as the texture to draw
    /// </summary>
    /// <param name="tex">The texture this <see cref="SpriteRenderer"/> will draw</param>
    public SpriteRenderer(Sprite tex) => this.tex = tex;
    /// <summary>
    /// Creates a new <see cref="SpriteRenderer"/> where the sprite will be loaded from your content folder
    /// </summary>
    /// <param name="contentPath">The path of where the sprite is you wish to load. excluding the file extention</param>
    public SpriteRenderer(string contentPath) : this(new Sprite(contentPath)) { }
    /// <summary>
    /// Creates a new <see cref="SpriteRenderer"/> where a new texture is created at runtime using the given parameters
    /// </summary>
    /// <param name="width">The width of the texture</param>
    /// <param name="height">The height of the texture</param>
    /// <param name="color">The color of the texture, format is hexadecimal (RED-GREEN-BLUE-ALPHA), the alpha part is optional, meaning "AABBCC" and "AABBCCDD" are both valid</param>
    public SpriteRenderer(int width, int height, string color) : this(MonoUtils.CreateTexture(width, height, color)) { }
    /// <summary>
    /// Creates a new <see cref="SpriteRenderer"/> where a new texture is created at runtime using the given parameters
    /// </summary>
    /// <param name="width">The width of the texture</param>
    /// <param name="height">The height of the texture</param>
    /// <param name="color">The color of the texture</param>
    public SpriteRenderer(int width, int height, Color color) : this(MonoUtils.CreateTexture(width, height, color)) { }

    public override void Render(SpriteBatch batch)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        Rectangle original = MonoUtils.Graphics.ScissorRectangle;
        if (ClipMask is not null && Camera.current is not null)
        {
            if (!batch.GraphicsDevice.RasterizerState.ScissorTestEnable)
                throw new Exception("SpriteRenderer wishes to clip rendering, but clipping wasnt enabled!");

            Rectangle mask = ClipMask.Value;

            // Calculate top-left of the sprite in world space
            Vector2 originOffset = new Vector2(
                tex.Width * Origin.X, tex.Height * Origin.Y);
            Vector2 worldTopLeft = transform.position - originOffset;

            // Add the clipmask relative to the sprite's top-left corner
            mask.Location += worldTopLeft.ToPoint();

            Debug.Log(mask);
            Debug.DrawRectangle(mask, Color.Red);
            Debug.DrawRectangle(Bounds, Color.GreenYellow);
            // Convert to screen space

            Rectangle r = Camera.current.WorldToScreenRectangle(mask);
            //r.Location += Camera.current.TopLeft.ToPoint();
            batch.GraphicsDevice.ScissorRectangle = r;

            Debug.Log(batch.GraphicsDevice.ScissorRectangle);
            Debug.DrawRectangle(batch.GraphicsDevice.ScissorRectangle, Color.Blue, 2, RenderSpace.Screen);
        }

        var a = GetTrueOrigin();
        batch.Draw(
            tex,
            transform.position,
            null,
            Tint,
            MathS.ToRadians(transform.rotation),
            a,
            transform.scale,
            Effects,
            LayerDepth
            );

        if (ClipMask is not null && Camera.current is not null)
        {
            MonoUtils.Graphics.ScissorRectangle = original;
            Debug.Log(MonoUtils.Graphics.ScissorRectangle);
        }
      
        sw.Stop();
        DrawTime = sw.Elapsed;
    }

    private Vector2 GetTrueOrigin()
    {
        return new(Origin.X * tex.Width, Origin.Y * tex.Height);
    }

    public void destroyImmediately() => owner.DestroyImmediately();
}
