using Raylib_cs;
using WinterRose.ForgeWarden.Input;

namespace WinterRose.ForgeWarden.UserInterface;

public abstract class UIContent
{
    public UIContainer owner { get; internal set; }

    public InputContext Input => owner.Input;

    public ContainerStyle Style => owner.Style;

    public bool IsHovered { get; internal set; }

    internal bool IsContentHovered(Rectangle contentBounds)
    {
        Vector2 mousePos = Input.MousePosition;
        return ray.CheckCollisionPointRec(mousePos, contentBounds);
    }

    public abstract Vector2 GetSize(Rectangle availableArea);
    protected internal abstract void Draw(Rectangle bounds);
    internal protected abstract float GetHeight(float maxWidth);

    protected internal virtual void Setup() { }
    protected internal virtual void Update() { }
    protected internal virtual void OnHover() { }
    protected internal virtual void OnHoverEnd() { }
    protected internal virtual void OnContentClicked(MouseButton button) { }
    protected internal virtual void OnOwnerClosing() { }
    protected internal virtual void Close()
    {
        owner.Close();
    }


}