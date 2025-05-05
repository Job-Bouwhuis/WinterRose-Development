using Microsoft.Xna.Framework.Graphics;
using System;

namespace WinterRose.Monogame;

/// <summary>
/// A default renderer that implements <see cref="ObjectComponent"/>. meaning it does not get an update loop callback, Use <see cref="ActiveRenderer"/> for this.
/// </summary>
public abstract class Renderer : ObjectComponent
{
    /// <summary>
    /// Whether the renderer should render its contents
    /// </summary>
    [IncludeInTemplateCreation, IncludeWithSerialization]
    public bool IsVisible { get; set; } = true;
    /// <summary>
    /// The bounds of this renderer
    /// </summary>
    [IncludeInTemplateCreation, IncludeWithSerialization]
    public abstract RectangleF Bounds { get; }

    /// <summary>
    /// The time it took to render the contents to the screen
    /// </summary>
    public abstract TimeSpan DrawTime { get; protected set; }

    /// <summary>
    /// When overriden, renders its contents to the provided <see cref="SpriteBatch"/> <paramref name="batch"/>
    /// </summary>
    /// <param name="batch"></param>
    public abstract void Render(SpriteBatch batch);
}
