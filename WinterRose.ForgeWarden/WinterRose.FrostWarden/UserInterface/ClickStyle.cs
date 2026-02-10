namespace WinterRose.ForgeWarden.UserInterface;

public enum ClickStyle
{
    /// <summary>
    /// The click is registered on mouse button release, and the content is considered clicked if the mouse click started on the content and is released while still hovering the content. This is the default and most common style for UI buttons, as it allows users to cancel a click by dragging away before releasing.
    /// </summary>
    Up,
    /// <summary>
    /// The click is registered on mouse button press if the press started while hovering the content, and the content is considered clicked as soon as the mouse button is pressed down, regardless of whether the mouse is released while still hovering the content.
    /// </summary>
    Down
}