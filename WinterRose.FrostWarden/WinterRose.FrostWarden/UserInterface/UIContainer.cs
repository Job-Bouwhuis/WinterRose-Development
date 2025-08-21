using Raylib_cs;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;

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

    bool initialized = false;

    internal void UpdateContainer()
    {
        if(!initialized)
        {
            initialized = true;
            foreach (var c in Contents)
                c.Setup();
        }
        Update();
    }

    protected virtual void Update()
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
        float hoverOffsetX = 0, hoverOffsetY = 0;
        float shadowOffsetX = 0, shadowOffsetY = 0;

        if(Style.RaiseOnHover)
        {
            bool isHovered = IsHovered();

            if (isHovered != isHoverTarget)
            {
                isHoverTarget = isHovered;
                hoverElapsed = 0f;
            }

            if (hoverElapsed < Style.RaiseDuration)
                hoverElapsed += Time.deltaTime;

            float tNormalized = Math.Clamp(hoverElapsed / Style.RaiseDuration, 0f, 1f);
            float easedProgress = Style.RaiseCurve?.Evaluate(isHoverTarget ? tNormalized : 1f - tNormalized) ?? (isHoverTarget ? 1f : 0f);

            hoverOffsetX = -4f * easedProgress;
            hoverOffsetY = -4f * easedProgress;
            shadowOffsetX = 4f * easedProgress;
            shadowOffsetY = 4f * easedProgress;
        }

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

        Rectangle bounds = new Rectangle(
            backgroundBounds.X + UIConstants.CONTENT_PADDING,
            backgroundBounds.Y + UIConstants.CONTENT_PADDING,
            backgroundBounds.Width - UIConstants.CONTENT_PADDING * 2,
            backgroundBounds.Height - UIConstants.CONTENT_PADDING * 2
        );

        DrawContent(bounds);
    }

    protected internal virtual void DrawContent(Rectangle contentArea)
    {
        float offsetY = contentArea.Y;
        foreach (var content in Contents)
        {
            float contentHeight = content.GetSize(contentArea).Y;
            Rectangle bounds = new Rectangle(contentArea.X, offsetY, contentArea.Width, contentHeight);
            content.InternalDraw(bounds);
            offsetY += contentHeight + UIConstants.CONTENT_PADDING;
        }
    }

    public virtual bool IsHovered() => Input.IsMouseHovering(CurrentPosition);

    public virtual void Close()
    {
        if (!IsClosing)
        {
            foreach (var c in Contents)
            {
                c.OnOwnerClosing();
            }

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
        content.Setup();
        return this;
    }

    public UIContainer AddButton(RichText text, ButtonClickHandler? onClick = null)
    {
        ButtonRowContent? btns = null;
        foreach (var item in Contents)
        {
            if (item is ButtonRowContent b)
            {
                btns = b;
                break;
            }
        }

        if (btns is null)
        {
            btns = new();
            AddContent(btns);
        }

        btns.AddButton(text, onClick);
        return this;
    }

    /// <summary>
    /// Adds a button to the toast
    /// </summary>
    /// <param name="text"></param>
    /// <param name="onClick">Should return true when the toast should close, false if not</param>
    /// <returns></returns>
    public UIContainer AddButton(string text, ButtonClickHandler? onClick) => AddButton(RichText.Parse(text, Color.White), onClick);

    /// <summary>
    /// Adds a progress bar to the toast
    /// </summary>
    /// <param name="initialProgress">The progress in a 0-1 range. set to -1 to have it do a infinite working animation</param>
    /// <param name="ProgressProvider">The function that provides further values</param>
    /// <param name="closesToastWhenComplete">When true, and the progress becomes 1, it requests the toast to close.</param>
    /// <returns></returns>
    public UIContainer AddProgressBar(float initialProgress, Func<float, float>? ProgressProvider = null, bool closesToastWhenComplete = true, string infiniteSpinText = "Working...")
    {
        return AddContent(new UIProgressContent(initialProgress, ProgressProvider, closesToastWhenComplete, infiniteSpinText));
    }

    /// <summary>
    /// Adds the sprite to the dialog
    /// </summary>
    /// <param name="sprite"></param>
    /// <returns></returns>
    public UIContainer AddSprite(Sprite sprite) => AddContent(new UISpriteContent(sprite));

    public UIContainer AddTitle(string text, UIFontSizePreset preset = UIFontSizePreset.Title)
    => AddText(RichText.Parse(text, Color.White), preset);
    public UIContainer AddTitle(RichText text, UIFontSizePreset preset = UIFontSizePreset.Title)
        => AddContent(new UIMessageContent(text, preset));
    public UIContainer AddText(RichText text, UIFontSizePreset preset = UIFontSizePreset.Message)
        => AddContent(new UIMessageContent(text, preset));

    public UIContainer AddText(string text, UIFontSizePreset preset = UIFontSizePreset.Message)
        => AddText(RichText.Parse(text, Color.White), preset);
}
