using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.UI;

/// <summary>
/// Renders on window space, instead of world space
/// </summary>
public abstract class UIRenderer : ObjectBehavior
{
    /// <summary>
    /// is called every frame to render this UI element
    /// </summary>
    public abstract void Render(SpriteBatch batch);

    /// <summary>
    /// The bounds of this UI element
    /// </summary>
    public abstract RectangleF Bounds { get; }
}
