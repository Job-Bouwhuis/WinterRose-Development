using Raylib_cs;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.UserInterface.Tooltipping;

namespace WinterRose.ForgeWarden.UserInterface;

public abstract class UIContent
{
    public IUIContainer Owner { get; internal set; }

    public InputContext Input => Owner?.Input ?? null;

    public ContentStyle Style => field ??= new ContentStyle(Owner.Style.StyleBase);

    public bool IsHovered { get; internal set; }

    public ClickStyle ClickStyle { get; set; } = ClickStyle.Up;

    private float hoverTimer;
    private Tooltip? spawnedTooltip;

    public VoidInvocation<Tooltip>? OnTooltipConfigure { get; set; }

    /// <summary>
    /// These bounds where used last frame to render this content
    /// </summary>
    public Rectangle LastRenderBounds { get; set; }

    internal bool IsContentHovered(Rectangle contentBounds, bool includeHoverExtenders = true)
    {
        Vector2 mousePos = Input.MousePosition;
        if (includeHoverExtenders && Tooltips.IsHoverExtended(this))
            return true;

        return ray.CheckCollisionPointRec(mousePos, contentBounds);
    }

    internal void InternalDraw(Rectangle bounds)
    {
        LastRenderBounds = bounds;
        Draw(bounds);
    }

    public abstract Vector2 GetSize(Rectangle availableArea);
    protected abstract void Draw(Rectangle bounds);
    internal protected abstract float GetHeight(float maxWidth);

    internal void _Update()
    {
        if (IsHovered && OnTooltipConfigure != null)
        {
            hoverTimer += Time.deltaTime;
            if (spawnedTooltip == null && hoverTimer >= Style.TooltipActivateTime)
            {
                spawnedTooltip = Tooltips.ForUIContent(this);
                OnTooltipConfigure?.Invoke(spawnedTooltip);
                Tooltips.Show(spawnedTooltip);
            }
        }
        else
        {
            hoverTimer = 0f;
            if(spawnedTooltip != null && spawnedTooltip.IsClosing == true)
                spawnedTooltip = null;
        }
        Update();
    }

    protected internal virtual void Setup() { }
    protected virtual void Update() { }
    protected internal virtual void OnHover() { }
    protected internal virtual void OnHoverEnd() { }
    protected internal virtual void OnContentClicked(MouseButton button) { }
    protected internal virtual void OnClickedOutsideOfContent(MouseButton button) { }
    protected internal virtual void OnOwnerClosing() { }
    protected internal void Close()
    {
        Owner.Close();
    }

}