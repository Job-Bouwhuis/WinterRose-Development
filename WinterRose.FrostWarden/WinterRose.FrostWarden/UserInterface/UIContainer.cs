using PuppeteerSharp;
using Raylib_cs;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace WinterRose.ForgeWarden.UserInterface;

public abstract class UIContainer
{
    // --- Input ---
    public abstract InputContext Input { get; }

    // --- Visibility & Lifecycle ---
    public bool IsClosing { get; internal set; }
    public bool IsVisible => !IsClosing;

    public bool IsBeingDragged { get; private set; } = false;
    /// <summary>
    /// Used for a preview. when unpaused, the container moves back to the mouse
    /// </summary>
    public bool PauseDragMovement { get; set; }
    private bool prevPauseDragMovement;
    internal Rectangle CurrentPosition;

    public Rectangle AllContentArea => new(
                CurrentPosition.X + UIConstants.CONTENT_PADDING,
                CurrentPosition.Y + UIConstants.CONTENT_PADDING,
                CurrentPosition.Width - UIConstants.CONTENT_PADDING * 2,
                CurrentPosition.Height - UIConstants.CONTENT_PADDING * 2
            );

    private float lastHeight = 0;
    public virtual float Height
    {
        get
        {
            float newHeight = UIConstants.CONTENT_PADDING + UIConstants.CONTENT_PADDING * (Contents.Count + 1) + dragHeight;
            if (newHeight != lastHeight && lastHeight is not 0)
                Toasts.RequestReorder();
            return lastHeight = newHeight;
        }
    }


    internal Vector2 CurrentScale;
    internal Vector2 TargetPosition;
    internal Vector2 TargetScale;
    internal float AnimationElapsed;

    protected bool IsDragTarget { get; private set; }
    private float dragHeight;

    private bool isHoverTarget = false;

    public bool PauseAutoDismissTimer { get; set; }
    public float TimeUntilAutoDismiss { get; set; } = 0;
    internal float TimeShown;

    public ContainerStyle Style { get; set; }

    internal List<UIContent> Contents { get; } = new();

    protected internal bool IsMorphDrawing { get; internal set; }
    protected Rectangle LastContentRenderBounds { get; private set; }
    public Rectangle LastBorderBounds { get; private set; }

    /// <summary>
    /// When true, The container will not attempt to move itself whenever TargetPosition changes. 
    /// <br></br> This can be useful when you want to manage the position of the container manually.
    /// </summary>
    public bool NoAutoMove { get; set; }

    bool initialized = false;

    internal void UpdateContainer()
    {
        if (!initialized)
        {
            initialized = true;
            foreach (var c in Contents)
                c.Setup();
        }
        Update();
    }

    protected virtual void OnContainerDragStart()
    {

    }

    protected virtual void OnContainerDragEnd()
    {

    }

    protected virtual void Update()
    {
        HandleMovement();

        HandleContentUpdates();
        HandleContainerDragging();
        HandleAutoClose();
    }

    protected internal virtual void HandleMovement()
    {
        if (!NoAutoMove)
        {
            AnimationElapsed += Time.deltaTime;
            float tNormalized = Math.Clamp(
                AnimationElapsed / (IsClosing ?
                                            Style.AnimateOutDuration
                                            : Style.AnimateInDuration), 0f, 1f);

            CurrentPosition.Position = Vector2.Lerp(CurrentPosition.Position, TargetPosition, Style.MoveAndScaleCurve.Evaluate(tNormalized));
            ComputeToastScale(tNormalized);
        }
    }

    private void ComputeToastScale(float tNormalized)
    {
        // store old center
        var center = new Vector2(
            CurrentPosition.X + CurrentPosition.Width / 2f,
            CurrentPosition.Y + CurrentPosition.Height / 2f
        );

        // lerp absolute width/height toward target
        CurrentScale = Vector2.Lerp(
            CurrentScale,
            TargetScale,
            Curves.EaseOutBackFar.Evaluate(tNormalized)
        );

        // recompute rect with locked center using absolute sizes
        float newWidth = CurrentScale.X;
        float newHeight = CurrentScale.Y;

        CurrentPosition = new Rectangle(
            center.X - newWidth / 2f,
            center.Y - newHeight / 2f,
            newWidth,
            newHeight
        );
    }

    protected virtual void HandleContainerDragging()
    {
        if (!Style.AllowDragging)
        {
            IsBeingDragged = false;
            IsDragTarget = false;
            return;
        }

        if (!IsBeingDragged)
        {
            if (!IsDragTarget)
                return; // if not drag target, skip dragging

            if (Input.IsPressed(MouseButton.Left))
            {
                // dragging initiated. this block is called once at the start of a drag
                OnContainerDragStart();
                PauseAutoDismissTimer = true;
                IsBeingDragged = true;
                Style.HoverRaiseAmount += Style.DragHoverRaiseExtra;

                float previousTotal = Style.HoverRaiseAmount;
                Style.HoverRaiseAmount += Style.DragHoverRaiseExtra; 
                Style.currentRaiseAmount *= previousTotal / Style.HoverRaiseAmount;
            }
            else
                return; // if not clicked, skip dragging
        }

        if (!Input.Provider.IsDown(new InputBinding(InputDeviceType.Mouse, (int)MouseButton.Left)))
        {
            IsBeingDragged = false;
            PauseAutoDismissTimer = false;
            Style.HoverRaiseAmount -= Style.DragHoverRaiseExtra;

            float previousTotal = Style.HoverRaiseAmount;
            Style.HoverRaiseAmount -= Style.DragHoverRaiseExtra;
            Style.currentRaiseAmount *= previousTotal / Style.HoverRaiseAmount;

            OnContainerDragEnd();
            return;
        }
        Style.currentDragDetectionAnimTime = 1;

        if (Style.RaiseOnHover)
        {
            Style.currentRaiseAmount = 1;
            isHoverTarget = true;
        }
        IsDragTarget = true;

        //Console.WriteLine(PauseAutoDismissTimer);
        if(prevPauseDragMovement != PauseAutoDismissTimer && !PauseAutoDismissTimer)
        {
            TargetPosition += Input.Provider.MousePosition;
            AnimationElapsed = 1 - AnimationElapsed;
        }

        prevPauseDragMovement = PauseAutoDismissTimer;

        if(!PauseDragMovement)
        {
            TargetPosition += Input.Provider.MouseDelta;
            AnimationElapsed = 1 - AnimationElapsed;
        }
    }

    private void HandleContentUpdates()
    {
        float time = IsDragTarget ? Style.currentDragDetectionAnimTime : 1f - Style.currentDragDetectionAnimTime;
        float contentOffsetY = CurrentPosition.Y + UIConstants.CONTENT_PADDING + time * Style.DragDetectionHeight;

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
    }

    protected internal virtual void Draw()
    {
        float hoverOffsetX = 0, hoverOffsetY = 0;
        float shadowOffsetX = 0, shadowOffsetY = 0;
        HandleRaiseAnimation(ref hoverOffsetX, ref hoverOffsetY, ref shadowOffsetX, ref shadowOffsetY);

        //float easedProgress = Style.RaiseCurve?.Evaluate(IsDragTarget ? Style.currentDragDetectionAnimTime : 1f - Style.currentDragDetectionAnimTime)
        //                ?? (IsDragTarget ? Style.currentDragDetectionAnimTime : 1f - Style.currentDragDetectionAnimTime);

        //hoverOffsetY += easedProgress * (Style.DragDetectionHeight + UIConstants.CONTENT_PADDING);

        var backgroundBounds = new Rectangle(
            CurrentPosition.X + hoverOffsetX,
            CurrentPosition.Y + hoverOffsetY,
            CurrentPosition.Width,
            CurrentPosition.Height);

        Rectangle dragBounds = new Rectangle(
                backgroundBounds.X + UIConstants.CONTENT_PADDING,
                backgroundBounds.Y,
                backgroundBounds.Width - UIConstants.CONTENT_PADDING * 2,
                Style.DragDetectionHeight + UIConstants.CONTENT_PADDING);
        backgroundBounds = HandleDragAcceptAnimationPart1(backgroundBounds, dragBounds);

        ray.DrawRectangleRec(new Rectangle(
            backgroundBounds.X - Style.ShadowSizeLeft + shadowOffsetX - hoverOffsetX,
            backgroundBounds.Y - Style.ShadowSizeTop + shadowOffsetY - hoverOffsetY,
            backgroundBounds.Width + Style.ShadowSizeLeft + Style.ShadowSizeRight,
            backgroundBounds.Height + Style.ShadowSizeTop + Style.ShadowSizeBottom + dragHeight),
            Style.Shadow
);

        ray.DrawRectangleRec(backgroundBounds, Style.Background);
        ray.DrawRectangleLinesEx(backgroundBounds, 2, Style.Border);
        HandleDragAcceptAnimationPart2(dragBounds);

        Rectangle bounds = new Rectangle(
            backgroundBounds.X + UIConstants.CONTENT_PADDING,
            backgroundBounds.Y + UIConstants.CONTENT_PADDING + dragHeight,
            backgroundBounds.Width - UIConstants.CONTENT_PADDING * 2,
            backgroundBounds.Height - UIConstants.CONTENT_PADDING * 2
        );

        LastContentRenderBounds = bounds;
        LastBorderBounds = backgroundBounds;
        DrawContent(bounds);

        if (TimeUntilAutoDismiss > 0)
            DrawCloseTimerBar(backgroundBounds);
    }

    private void DrawCloseTimerBar(Rectangle r)
    {
        float progressRatio = Math.Clamp(TimeShown / TimeUntilAutoDismiss, 0f, 1f);
        float barHeight = 3f; // adjustable height
        float yPos = r.Y + (r.Height - 2) - barHeight;

        // Background
        ray.DrawRectangle(
            (int)r.X + 2,
            (int)yPos,
            (int)r.Width - 4,
            (int)barHeight,
            Style.TimerBarBackground
        );

        // Fill
        ray.DrawRectangle(
            (int)r.X + 2,
            (int)yPos,
            (int)((r.Width - 4) * progressRatio),
            (int)barHeight,
            Style.TimerBarFill
        );
    }

    private void HandleDragAcceptAnimationPart2(Rectangle dragBounds)
    {
        if (Style.AllowDragging && Style.currentDragDetectionAnimTime > 0)
        {
            float availableHeight = dragBounds.Height + UIConstants.CONTENT_PADDING;
            float availableWidth = dragBounds.Width;

            Rectangle textSize = Style.DragHintText.CalculateBounds(availableWidth);

            int fontSize = (int)Math.Floor(Math.Min(
                availableHeight + UIConstants.CONTENT_PADDING * 0.6f,
                availableWidth / textSize.Width * 1.8f
            ));

            if (fontSize > 4)
            {
                Vector2 textPos = new Vector2(
                    dragBounds.X + (dragBounds.Width - textSize.Width) / 2f,
                    dragBounds.Y + (dragBounds.Height - textSize.Height) / 2f + UIConstants.CONTENT_PADDING
                );
                float alpha = IsDragTarget ? Style.currentDragDetectionAnimTime : 1f - Style.currentDragDetectionAnimTime;

                RichTextRenderer.DrawRichText(
                    Style.DragHintText,
                    textPos,
                    dragBounds.Width,
                    Color.White.WithAlpha(alpha),
                    null);
            }
        }
    }

    private Rectangle HandleDragAcceptAnimationPart1(Rectangle backgroundBounds, Rectangle dragBounds)
    {
        if (Style.AllowDragging || Style.currentDragDetectionAnimTime > 0f)
        {
            bool hoveringDragTarget = false;
            if (Style.AllowDragging)
                hoveringDragTarget = ray.CheckCollisionPointRec(Input.MousePosition, dragBounds);

            if (hoveringDragTarget != IsDragTarget)
            {
                IsDragTarget = hoveringDragTarget;
                Style.currentDragDetectionAnimTime = 1f - Style.currentDragDetectionAnimTime;
            }

            Style.currentDragDetectionAnimTime += Time.deltaTime / Style.DragDetectionAreaHeightChangeDuration;
            Style.currentDragDetectionAnimTime = Math.Clamp(Style.currentDragDetectionAnimTime, 0f, 1f);

            // Eased progress for the current hover state
            float easedProgress = Style.RaiseCurve?.Evaluate(IsDragTarget ? Style.currentDragDetectionAnimTime : 1f - Style.currentDragDetectionAnimTime)
                                ?? (IsDragTarget ? Style.currentDragDetectionAnimTime : 1f - Style.currentDragDetectionAnimTime);

            dragHeight = (Style.DragDetectionHeight + UIConstants.CONTENT_PADDING) * easedProgress;

            AlterBoundsCorrectlyForDragBar(ref backgroundBounds, dragHeight);

            if (Style.currentDragDetectionAnimTime.Round(4) is > 0.9950f or < 0.0050f)
                Toasts.RequestReorder();
        }

        return backgroundBounds;
    }

    protected abstract void AlterBoundsCorrectlyForDragBar(ref Rectangle backgroundBounds, float dragHeight);

    private void HandleRaiseAnimation(ref float hoverOffsetX, ref float hoverOffsetY, ref float shadowOffsetX, ref float shadowOffsetY)
    {
        if (Style.RaiseOnHover || Style.currentRaiseAmount != 0)
        {
            bool isHovered = IsHovered();

            if (isHovered != isHoverTarget)
            {
                isHoverTarget = isHovered;
                Style.currentRaiseAmount = 0f;
            }

            if (Style.currentRaiseAmount < Style.RaiseDuration)
                Style.currentRaiseAmount += Time.deltaTime;

            float tNormalized = Math.Clamp(Style.currentRaiseAmount / Style.RaiseDuration, 0f, 1f);
            float easedProgress = Style.RaiseCurve?.Evaluate(isHoverTarget ? tNormalized : 1f - tNormalized) ?? (isHoverTarget ? 1f : 0f);

            hoverOffsetX = -Style.HoverRaiseAmount * easedProgress;
            hoverOffsetY = -Style.HoverRaiseAmount * easedProgress;
            shadowOffsetX = Style.HoverRaiseAmount * easedProgress;
            shadowOffsetY = Style.HoverRaiseAmount * easedProgress;
        }
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
