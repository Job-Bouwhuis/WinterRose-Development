using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace WinterRose.Monogame;



/// <summary>
/// A SpriteRenderer that automatically updates every frame
/// </summary>
public sealed class AnimatedSpriteRenderer : ActiveRenderer
{
    /// <summary>
    /// The <see cref="SpriteEffects"/> to use when rendering
    /// </summary>
    public SpriteEffects Effects { get; set; }
    /// <summary>
    /// The current index of which sprite to use
    /// </summary>
    [IncludeInTemplateCreation] public int CurrentSpriteIndex { get; set; } = -1;
    /// <summary>
    /// Layerdepth, used to determain what render element should be drawn in front of another
    /// </summary>
    [IncludeInTemplateCreation] public float LayerDepth { get; set; } = 0.5f;
    /// <summary>
    /// Whether to run the update loop, can be used to pause the animation
    /// </summary>
    [IncludeInTemplateCreation] public bool AutoUpdate { get; set; } = true;
    /// <summary>
    /// The amount of time between each frame in the animation
    /// </summary>
    [IncludeInTemplateCreation] public float SpriteStepTime { get; set; } = 0.5f;
    /// <summary>
    /// Set this to True to have it start over when it reaches the end of the <see cref="SpriteSheet"/>. if False, it stops at the last frame and keeps it there
    /// </summary>
    [IncludeInTemplateCreation] public bool Loop { get; set; } = true;
    /// <summary>
    /// The sprite sheet where the sprites will be taken from
    /// </summary>
    [IncludeInTemplateCreation] public SpriteSheet SpriteSheet { get; set; }
    /// <summary>
    /// The draw origin of this sprite, values between 0 and 1 are considered inside the bounds of the sprite. values outside 0 and 1 are accepted
    /// </summary>
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
    /// The bounds of the sprite that is currently being drawn
    /// </summary>
    public override RectangleF Bounds
    {
        get
        {
            return new(CurrentSprite.Bounds.Width.Round(), CurrentSprite.Bounds.Height.Round(), 0, 0);
        }
    }
    /// <summary>
    /// The current sprite that is being drawn
    /// </summary>
    public Sprite CurrentSprite => SpriteSheet?[CurrentSpriteIndex];
    /// <summary>
    /// The rectangle reference on the spritesheet where the data for the current texture is located
    /// </summary>
    public Rectangle? CurrentRectangle => SpriteSheet?.GetRectangle(CurrentSpriteIndex);

    public override TimeSpan DrawTime { get; protected set; }

    private Vector2 origin = new(0.5f, 0.5f);
    private float waitTime = 0;

    /// <summary>
    /// Creates a new <see cref="AnimatedSpriteRenderer"/> that has a sprite sheet selected
    /// </summary>
    /// <param name="sheet"></param>
    public AnimatedSpriteRenderer(SpriteSheet sheet) : this()
    {
        SpriteSheet = sheet;
    }
    /// <summary>
    /// Creates a new <see cref="AnimatedSpriteRenderer"/> with no sprite sheet. you can set it later by setting the <see cref="SpriteSheet"/> property of this instances
    /// </summary>
    public AnimatedSpriteRenderer() => waitTime = SpriteStepTime;

    protected override void Update()
    {
        if (!Loop && CurrentSpriteIndex >= SpriteSheet.SpriteCount)
            return;

        if (!AutoUpdate) return;

        if (waitTime >= SpriteStepTime)
        {
            CurrentSpriteIndex++;
            if (CurrentSpriteIndex >= SpriteSheet.SpriteCount)
                if (Loop)
                    CurrentSpriteIndex = 0;
                else
                    CurrentSpriteIndex--;

            waitTime = 0;
        }
        else
            waitTime += Time.deltaTime;
    }
    public override void Render(SpriteBatch batch)
    {
        var sw = Stopwatch.StartNew();
        if (CurrentRectangle is null)
            return;

        var trueOrigin = GetTrueOrigin();
        batch.Draw(
            SpriteSheet.SourceSprite,
            transform.position,
            CurrentRectangle,
            Color.White,
            MathS.ToRadians(transform.rotation),
            trueOrigin,
            transform.scale,
            Effects,
            LayerDepth
            );
        sw.Stop();
        DrawTime = sw.Elapsed;
    }
    private Vector2 GetTrueOrigin()
    {
        return new((Origin.X * CurrentRectangle?.Width) ?? 0, (Origin.Y * CurrentRectangle?.Height) ?? 0);
    }

}
