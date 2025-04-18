using Microsoft.Xna.Framework.Graphics;
using System;
using WinterRose.Serialization;

namespace WinterRose.Monogame;

/// <summary>
/// A renderer that implements <see cref="ObjectBehavior"/> and thus has a update callback
/// </summary>
public abstract class ActiveRenderer : ObjectBehavior
{
    /// <summary>
    /// Should the renderer draw render its contents to the screen
    /// </summary>
    [IncludeInTemplateCreation, IncludeWithSerialization]
    public bool IsVisible { get; set; } = true;
    /// <summary>
    /// The bounds of this renderer
    /// </summary>
    public abstract RectangleF Bounds { get; }

    /// <summary>
    /// The time it took to render the contents to the screen
    /// </summary>
    [ExcludeFromSerialization]
    public abstract TimeSpan DrawTime { get; protected set; }

    /// <summary>
    /// When overriden, renders its contents to the provided <see cref="SpriteBatch"/>
    /// </summary>
    /// <param name="batch"></param>
    public abstract void Render(SpriteBatch batch);
}
