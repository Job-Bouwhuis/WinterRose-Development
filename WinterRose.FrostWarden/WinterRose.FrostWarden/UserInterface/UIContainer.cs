using Raylib_cs;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.UserInterface;

public abstract class UIContainer
{
    // --- Input ---
    public abstract InputContext Input { get; }

    // --- Visibility & Lifecycle ---
    public bool IsClosing { get; internal set; }
    public bool IsVisible => !IsClosing;

    // --- Animation & Position ---
    internal Rectangle CurrentPosition;

    public Rectangle ContentArea => new(
                CurrentPosition.X + UIConstants.CONTENT_PADDING,
                CurrentPosition.Y + UIConstants.CONTENT_PADDING,
                CurrentPosition.Width - UIConstants.CONTENT_PADDING * 2,
                CurrentPosition.Height - UIConstants.CONTENT_PADDING * 2
            );


    internal Vector2 CurrentScale;
    internal Vector2 TargetPosition;
    internal Vector2 TargetScale;
    internal float AnimationElapsed;

    private float hoverElapsed = 0f;
    private bool isHoverTarget = false;

    public bool PauseAutoDismissTimer { get; set; }
    public float TimeUntilAutoDismiss { get; set; } = 0;
    internal float TimeShown;

    public ContainerStyle Style { get; set; }

    internal List<UIContent> Contents { get; } = new();

    protected internal bool IsMorphDrawing { get; internal set; }

    internal Rectangle LastScaledBoundingBox { get; set; }

    protected internal virtual void Update()
    {
        float contentOffsetY = CurrentPosition.Y + UIConstants.CONTENT_PADDING;

        foreach (var content in Contents)
        {
            float contentHeight = content.GetHeight(CurrentPosition.Width);
            Rectangle contentBounds = new Rectangle(
                CurrentPosition.X + UIConstants.CONTENT_PADDING,
                contentOffsetY,
                CurrentPosition.Width - UIConstants.CONTENT_PADDING * 2,
                contentHeight
            );

            if (content.IsContentHovered(contentBounds))
            {
                content.OnHover();
                content.IsHovered = true;

                foreach (var button in Enum.GetValues<MouseButton>())
                {
                    if (Input.IsPressed(button))
                        content.OnContentClicked(button);
                }
            }
            else
            {
                if (content.IsHovered)
                    content.OnHoverEnd();
                content.IsHovered = false;
            }

            content.Update();
            contentOffsetY += contentHeight + UIConstants.CONTENT_PADDING;
        }

        HandleAutoClose();
    }

    protected internal virtual void Draw()
    {
        bool isHovered = IsHovered();

        if (isHovered != isHoverTarget)
        {
            isHoverTarget = isHovered;
            hoverElapsed = 0f;
        }

        if (hoverElapsed < Style.HoverDuration)
            hoverElapsed += Time.deltaTime;

        float tNormalized = Math.Clamp(hoverElapsed / Style.HoverDuration, 0f, 1f);
        float easedProgress = Style.HoverCurve?.Evaluate(isHoverTarget ? tNormalized : 1f - tNormalized) ?? (isHoverTarget ? 1f : 0f);

        float hoverOffsetX = -4f * easedProgress;
        float hoverOffsetY = -4f * easedProgress;
        float shadowOffsetX = 4f * easedProgress;
        float shadowOffsetY = 4f * easedProgress;

        // Shadow
        ray.DrawRectangleRec(new Rectangle(
            CurrentPosition.X - Style.ShadowSizeLeft + shadowOffsetX,
            CurrentPosition.Y - Style.ShadowSizeTop + shadowOffsetY,
            CurrentPosition.Width + Style.ShadowSizeLeft + Style.ShadowSizeRight,
            CurrentPosition.Height + Style.ShadowSizeTop + Style.ShadowSizeBottom),
            Style.Shadow
        );

        // Background
        var backgroundBounds = new Rectangle(
            CurrentPosition.X + hoverOffsetX,
            CurrentPosition.Y + hoverOffsetY,
            CurrentPosition.Width,
            CurrentPosition.Height
        );

        ray.DrawRectangleRec(backgroundBounds, Style.Background);
        ray.DrawRectangleLinesEx(backgroundBounds, 2, Style.Border);

        DrawContent(new Rectangle(
            backgroundBounds.X + UIConstants.CONTENT_PADDING,
            backgroundBounds.Y + UIConstants.CONTENT_PADDING,
            backgroundBounds.Width - UIConstants.CONTENT_PADDING * 2,
            backgroundBounds.Height - UIConstants.CONTENT_PADDING * 2
        ));
    }

    protected internal virtual void DrawContent(Rectangle contentArea)
    {
        float offsetY = contentArea.Y;
        foreach (var content in Contents)
        {
            float contentHeight = content.GetSize(contentArea).Y;
            Rectangle bounds = new Rectangle(contentArea.X, offsetY, contentArea.Width, contentHeight);
            content.Draw(bounds);
            offsetY += contentHeight + UIConstants.CONTENT_PADDING;
        }
    }

    public virtual bool IsHovered() => Input.IsMouseHovering(CurrentPosition);

    public virtual void Close()
    {
        if (!IsClosing)
        {
            IsClosing = true;
            isHoverTarget = true;
            AnimationElapsed = 0f;
        }
    }

    protected virtual void HandleAutoClose()
    {
        if (!PauseAutoDismissTimer && TimeUntilAutoDismiss > 0 && !IsHovered())
        {
            TimeShown += Time.deltaTime;
            if (TimeShown >= TimeUntilAutoDismiss)
                Close();
        }
    }

    public virtual UIContainer AddContent(UIContent content)
    {
        Contents.Add(content);
        content.owner = this;
        return this;
    }
}
