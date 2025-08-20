using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.ToastNotifications;
public abstract class ToastContent
{
    internal Toast owner;
    public ToastStyle Style => owner.Style;

    public abstract float GetHeight(float width);

    /// <summary>
    /// Called each frame to update internal logic (timers, animations)
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// Called when this content is clicked.
    /// </summary>
    public virtual void OnClick(MouseButton button) { }

    /// <summary>
    /// Called when this content is hovered.
    /// </summary>
    public virtual void OnHover() { }

    /// <summary>
    /// Draw the content in the given bounds.
    /// </summary>
    /// <param name="bounds">The calculated area that is allotted to this content. Its based on <see cref="GetHeight(float)"/></param>
    public abstract void Draw(Rectangle bounds, float contentAlpha);

    /// <summary>
    /// Optional helper: checks if the mouse is over this content's bounds.
    /// </summary>
    public bool IsHovered(Rectangle bounds)
    {
        Vector2 mousePos = ray.GetMousePosition();
        return ray.CheckCollisionPointRec(mousePos, bounds);
    }

    /// <summary>
    /// Called when the toast this content is on is closing
    /// </summary>
    public virtual void OnToastClosing()
    {

    }

    /// <summary>
    /// Closes the toast this content is on
    /// </summary>
    public void Close() => owner.Close();
}
