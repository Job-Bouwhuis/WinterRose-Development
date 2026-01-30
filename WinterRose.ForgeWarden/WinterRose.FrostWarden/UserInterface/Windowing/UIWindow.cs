using Raylib_cs;
using System;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Tweens;

namespace WinterRose.ForgeWarden.UserInterface.Windowing;

public class UIWindow : UIContainer
{
    private enum PendingAction
    {
        None,
        MaximizeAfterExpand,
        CollapseAfterUnmaximize
    }

    private enum ShowMode
    {
        Standard,
        Collapsed,
        Maximized
    }

    private PendingAction pendingAction = PendingAction.None;
    private ShowMode showMode = ShowMode.Standard;

    private InputContext input = new(new RaylibInputProvider(), -10000, false);
    private RichText title;

    private float collapseProgress = 0f;      // 0 = full, 1 = collapsed
    private float collapseTarget = 0f;
    private const float COLLAPSE_DURATION = 0.22f;

    private float maximizeProgress = 0f;      // 0 = normal size, 1 = normal maximized
    private float maximizeTarget = 0f;
    private const float MAXIMIZE_DURATION = 0.22f;
    private bool maximizeUseMouseTarget = false;
    private Vector2 maximizeMouseTarget = new Vector2();

    private float originalContentAlpha = 1f;
    private Rectangle fullPosCache;

    // closing state
    public bool IsClosed { get; private set; } = false;
    /// <summary>
    /// Set by the window when the full close animation has been completed, and should be removed from the list of windows
    /// </summary>
    public bool IsFullyClosed { get; private set; } = false;

    private bool closeAfterCollapse = false;
    private bool closeAnimationActive = false;
    private float closeProgress = 0f;
    private const float CLOSE_DURATION = 0.44f;

    private bool hasBeenShown = false;
    private bool openAnimationActive = false;
    private float openProgress = 0f;
    private const float OPEN_DURATION = 0.18f;


    private Vector2 closeStartSize;
    private float closeStartContentAlpha;

    public override InputContext Input => input;

    // --- Collapsing state ---
    private bool isCollapsed = false;
    public bool Collapsed
    {
        get => isCollapsed;
        set
        {
            // If maximized (or animating toward maximizing), unmaximize first then collapse
            bool currentlyMaximized = isMaximized || maximizeAnimationActive || (maximizeProgress > 0f && maximizeTarget > 0f);
            if (currentlyMaximized)
            {
                pendingAction = PendingAction.CollapseAfterUnmaximize;
                Maximized = false; // start unmaximize animation
                return;
            }
            if(value)
                fullPosCache = CurrentPosition;

            if (isCollapsed == value) return;
            isCollapsed = value;
            collapseTarget = isCollapsed ? 1f : 0f;
        }
    }

    private Vector2 sizeBeforeMaximize;
    private Vector2 positionBeforeMaximize;
    private Vector2 maximizeAnimStartPos;
    private Vector2 maximizeAnimStartSize;
    private Vector2 maximizeAnimEndPos;
    private Vector2 maximizeAnimEndSize;
    private bool maximizeRequested = false;
    private bool maximizeRequestedValue = false;
    private bool maximizeFinalizeTargetIsMaximized = false;
    private bool maximizeAnimationActive = false;
    private bool isMaximized = false;
    public bool Maximized
    {
        get => isMaximized;
        set
        {
            bool currentlyCollapsed = isCollapsed || (collapseProgress > 0f && collapseTarget > 0f);
            if (currentlyCollapsed)
            {
                pendingAction = PendingAction.MaximizeAfterExpand;
                Collapsed = false;
                return;
            }

            if (maximizeAnimationActive)
            {
                maximizeRequested = true;
                maximizeRequestedValue = value;
                return;
            }

            if (isMaximized == value) return;

            maximizeFinalizeTargetIsMaximized = value;

            maximizeTarget = value ? 1f : 0f;
            maximizeAnimStartPos = CurrentPosition.Position;
            maximizeAnimStartSize = CurrentPosition.Size;

            if (value)
            {
                positionBeforeMaximize = maximizeAnimStartPos;
                sizeBeforeMaximize = maximizeAnimStartSize;

                maximizeAnimEndPos = new Vector2(0f, 0f);
                maximizeAnimEndSize = new Vector2(ForgeWardenEngine.Current.Window.Width, ForgeWardenEngine.Current.Window.Height);

                NoAutoMove = true;
            }
            else
            {
                // If a mouse target was captured (user clicked while maximized) compute an
                // end position so the restored window's titlebar ends up under the mouse.
                if (maximizeUseMouseTarget && sizeBeforeMaximize.X > 0 && sizeBeforeMaximize.Y > 0)
                {
                    var mouse = maximizeMouseTarget;
                    float targetWidth = sizeBeforeMaximize.X;
                    float targetHeight = sizeBeforeMaximize.Y;

                    // place window so mouse is centered on the titlebar horizontally,
                    // and vertically place mouse in middle of titlebar (so the mouse is "over" it)
                    float targetX = mouse.X - targetWidth * 0.5f;
                    float targetY = mouse.Y - (Style.TitleBarHeight * 0.5f);

                    // clamp to screen bounds
                    var win = ForgeWardenEngine.Current.Window;
                    targetX = MathF.Max(0f, MathF.Min(targetX, win.Width - targetWidth));
                    targetY = MathF.Max(0f, MathF.Min(targetY, win.Height - targetHeight));

                    maximizeAnimEndPos = new Vector2(targetX, targetY);
                    maximizeAnimEndSize = new Vector2(targetWidth, targetHeight);

                    // consume the mouse-target flag
                    maximizeUseMouseTarget = false;
                }
                else
                {
                    maximizeAnimEndPos = positionBeforeMaximize;
                    maximizeAnimEndSize = sizeBeforeMaximize;

                    if (maximizeAnimEndSize.X == 0f && maximizeAnimEndSize.Y == 0f)
                    {
                        // fallback if we somehow have no stored pre-maximize geometry
                        maximizeAnimEndPos = maximizeAnimStartPos;
                        maximizeAnimEndSize = maximizeAnimStartSize;
                    }
                }

                NoAutoMove = true;
            }

            maximizeAnimationActive = true;
        }
    }

    public Vector2 Position
    {
        get => CurrentPosition.Position;
        set
        {
            SetPosition(TargetPosition = value);
        }
    }

    public Vector2 Size
    {
        get => CurrentPosition.Size;
        set => SetSize(TargetSize = value);
    }

    public new WindowStyle Style
    {
        get => (WindowStyle)base.Style;
        set => base.Style = value;
    }

    private bool collapseAnimationFinished;

    //public override float Height => CurrentPosition.Size.Y + base.Height;

    public RichText Title
    {
        get => title;
        set => title = value ?? "";
    }
    public MulticastVoidInvocation<UIWindow> OnClosing { get; } = new();

    public UIWindow(RichText title, float width, float height) : this(title, width, height, 10, 10) { }

    public UIWindow(RichText title, float width, float height, float x, float y)
    {
        Style = new WindowStyle(new StyleBase());
        Size = new Vector2(width, height);
        Position = new(x, y);
        Title = title;

        originalContentAlpha = Style.ContentAlpha;
        fullPosCache = new(x, y, width, height);
    }

    public void ToggleCollapsed()
    {
        Collapsed = !isCollapsed;
    }

    public void ToggleMaximized()
    {
        Maximized = !isMaximized;
    }

    public virtual void Show()
    {
        WindowManager.Show(this);

        if (!hasBeenShown)
        {
            ResetState();
            hasBeenShown = true;
            StartOpenSequence();
        }
    }

    public void ShowCollapsed()
    {
        WindowManager.Show(this);

        if (!hasBeenShown)
        {
            ResetState();
            hasBeenShown = true;
            StartOpenSequence();
            showMode = ShowMode.Collapsed;
        }
    }

    public void ShowMaximized()
    {
        WindowManager.Show(this);

        if (!hasBeenShown)
        {
            ResetState();
            hasBeenShown = true;
            StartOpenSequence();
            showMode = ShowMode.Maximized;
        }
    }

    private void StartOpenSequence()
    {
        if (openAnimationActive || IsFullyClosed) return;

        IsClosed = false;
        IsFullyClosed = false;
        closeAfterCollapse = false;
        closeAnimationActive = false;

        CollapseImmediately();

        Style.ContentAlpha = 0f;

        openAnimationActive = true;
        openProgress = 0f;

        NoAutoMove = true;
    }

    public void ResetState()
    {
        collapseProgress = 0f;
        collapseTarget = 0f;

        maximizeProgress = 0f;
        maximizeTarget = 0f;

        showMode = ShowMode.Standard;

        IsClosed = false;
        IsFullyClosed = false;
        IsClosing = false;

        CurrentPosition = fullPosCache;
        TargetPosition = fullPosCache.Position;
        TargetSize = fullPosCache.Size;

        closeAfterCollapse = false;
        closeAnimationActive = false;
        closeProgress = 0f;
        pendingAction = PendingAction.None;

        AnimationElapsed = 0;

        hasBeenShown = false;
        openAnimationActive = false;
        openProgress = 0f;

        NoAutoMove = false;
    }

    public override bool IsHovered()
    {
        if (collapseProgress <= 0f)
            return base.IsHovered();

        // use the same easing/height as Update() so hover semantics match the visual collapsed height
        float eased = Curves.EaseOutBackFar?.Evaluate(collapseProgress) ?? collapseProgress;
        float collapsedHeight = UIConstants.CONTENT_PADDING * 2f + (Style.AllowUserResizing ? Style.TitleBarHeight : 0f);
        float animatedHeight = Lerp(fullPosCache.Height, collapsedHeight, eased);

        var bgRect = new Rectangle(CurrentPosition.X, CurrentPosition.Y, CurrentPosition.Width, animatedHeight);
        return Input.IsMouseHovering(bgRect);
    }

    protected override bool DrawCustomTitleBar(Rectangle titlebarBounds)
    {
        // --- layout constants ---
        const float BUTTON_SIZE_RATIO = 0.8f; // fraction of titlebar height
        const int BUTTON_PADDING = 6;
        const int BUTTON_SPACING = 6;
        const float ICON_INSET_RATIO = 0.25f; // inset for icon drawing inside the button

        // --- draw title bar background ---
        Raylib.DrawRectangleRec(titlebarBounds, Style.TitleBarBackground);

        // --- compute button sizes and rects (right aligned) ---
        ComputeButtonRects(titlebarBounds, BUTTON_SIZE_RATIO, BUTTON_PADDING, BUTTON_SPACING,
                           out int buttonSize, out Rectangle closeRect, out Rectangle maximizeRect, out Rectangle collapseRect, out float right);

        // --- mouse state (use Input for reliable coordinates) ---
        GetMouseStates(closeRect, maximizeRect, collapseRect,
                       out Vector2 mousePos, out bool overClose, out bool overMaximize, out bool overCollapse,
                       out bool mouseReleased, out bool mouseDown, out bool windowHasMouseFocus);

        // --- draw buttons ---
        var closeBg = ChooseColor(Style.CloseButtonBackground, Style.CloseButtonHover, Style.CloseButtonClick, overClose, mouseDown && overClose);
        DrawCloseButton(closeRect, buttonSize, ICON_INSET_RATIO, closeBg);

        var maxBg = ChooseColor(Style.MaximizeButtonBackground, Style.MaximizeButtonHover, Style.MaximizeButtonClick, overMaximize, mouseDown && overMaximize);
        DrawMaximizeButton(maximizeRect, buttonSize, ICON_INSET_RATIO, maxBg);

        var collapseBg = ChooseColor(Style.CollapseButtonBackground, Style.CollapseButtonHover, Style.CollapseButtonClick, overCollapse, mouseDown && overCollapse);
        DrawCollapseButton(collapseRect, buttonSize, ICON_INSET_RATIO, collapseBg);

        // --- title text: adjust font size to fit available area and draw ---
        float leftPadding = titlebarBounds.X + BUTTON_PADDING;

        float maxTextWidth;
        if (Style.ShowCollapseButton)
            maxTextWidth = collapseRect.X - leftPadding - 8;
        else if (Style.ShowMaximizeButton)
            maxTextWidth = maximizeRect.X - leftPadding - 8;
        else if (Style.ShowCloseButton)
            maxTextWidth = closeRect.X - leftPadding - 8;
        else
            maxTextWidth = titlebarBounds.Width - 8;


        if (maxTextWidth < 8) maxTextWidth = 8;

        float maxFontSize = titlebarBounds.Height * 0.6f;
        float minFontSize = 8f;

        float chosenFontSize = ComputeAndApplyBestFontSize(titlebarBounds, maxTextWidth, maxFontSize, minFontSize);

        // draw the rich text; align vertically centered
        var textPos = new Vector2(leftPadding, titlebarBounds.Y + (titlebarBounds.Height - (int)(chosenFontSize)) / 2f - 1f);
        RichTextRenderer.DrawRichText(Title, textPos, maxTextWidth, Style.White, null);

        // --- interaction: if any button was released while mouse was over it AND this window has mouse focus,
        // return false so the base dragging logic won't run. (Caller can handle the actual action elsewhere.) ---
        if (windowHasMouseFocus)
        {
            if (overClose || overMaximize || overCollapse)
            {
                // perform collapse immediately when collapse button clicked
                if (overCollapse && mouseReleased)
                    ToggleCollapsed();
                if (overMaximize && mouseReleased)
                    ToggleMaximized();
                if (overClose && mouseReleased)
                    Close();

                // a button was activated — prevent propagation to dragging
                return false;
            }
        }

        // no custom interaction — allow normal dragging behavior
        return true;
    }

    protected override void AfterResize()
    {
        positionBeforeMaximize = CurrentPosition.Position;
        sizeBeforeMaximize = CurrentPosition.Size;
        fullPosCache = CurrentPosition;
    }

    private void ComputeButtonRects(Rectangle titlebarBounds, float buttonSizeRatio, int buttonPadding, int buttonSpacing,
                                    out int buttonSize, out Rectangle closeRect, out Rectangle maximizeRect, out Rectangle collapseRect, out float right)
    {
        float buttonSizeF = titlebarBounds.Height * buttonSizeRatio;
        buttonSize = (int)MathF.Max(12, buttonSizeF);

        // start at right edge
        right = titlebarBounds.X + titlebarBounds.Width - buttonPadding;

        closeRect = maximizeRect = collapseRect = new();

        if (Style.ShowCloseButton)
        {
            closeRect = new Rectangle(right - buttonSize, titlebarBounds.Y + (titlebarBounds.Height - buttonSize) / 2f, buttonSize, buttonSize);
            right -= (buttonSize + buttonSpacing);
        }

        if (Style.ShowMaximizeButton)
        {
            maximizeRect = new Rectangle(right - buttonSize, titlebarBounds.Y + (titlebarBounds.Height - buttonSize) / 2f, buttonSize, buttonSize);
            right -= (buttonSize + buttonSpacing);
        }

        if (Style.ShowCollapseButton)
        {
            collapseRect = new Rectangle(right - buttonSize, titlebarBounds.Y + (titlebarBounds.Height - buttonSize) / 2f, buttonSize, buttonSize);
            right -= (buttonSize + buttonSpacing);
        }
    }


    private void GetMouseStates(Rectangle closeRect, Rectangle maximizeRect, Rectangle collapseRect,
                                out Vector2 mousePos, out bool overClose, out bool overMaximize, out bool overCollapse,
                                out bool mouseReleased, out bool mouseDown, out bool windowHasMouseFocus)
    {
        mousePos = Input.MousePosition;
        overClose = Raylib.CheckCollisionPointRec(mousePos, closeRect);
        overMaximize = Raylib.CheckCollisionPointRec(mousePos, maximizeRect);
        overCollapse = Raylib.CheckCollisionPointRec(mousePos, collapseRect);

        mouseReleased = Input.IsUp(MouseButton.Left);
        mouseDown = Input.IsDown(MouseButton.Left);

        windowHasMouseFocus = Input.HasMouseFocus;
    }

    private static Color ChooseColor(Color normal, Color hover, Color click, bool hoverState, bool clickState)
    {
        if (clickState) return click;
        if (hoverState) return hover;
        return normal;
    }

    protected override void DrawVerticalScrollbar(Rectangle contentArea, Rectangle backgroundBounds)
    {
        if (!isCollapsed && collapseProgress is <= 0f)
            base.DrawVerticalScrollbar(contentArea, backgroundBounds);
    }

    private Vector2 maximizeDragStart = Vector2.Zero;
    private bool maximizeDragArmed = false;
    private const float UNMAX_DRAG_THRESHOLD = 8f;
    private bool drawMaximizeDragBar = false;


    protected override void HandleContainerDragging()
    {
        if (Maximized)
        {
            if (!IsDragTarget)
                return;

            // On initial press, record the start position
            if (Input.IsPressed(MouseButton.Left))
            {
                maximizeDragStart = Input.MousePosition;
                maximizeDragArmed = true;
            }

            // If we're armed and mouse moved enough, trigger unmaximize
            if (maximizeDragArmed && Input.IsDown(MouseButton.Left))
            {
                float dx = Math.Abs(Input.MousePosition.X - maximizeDragStart.X);
                float dy = Math.Abs(Input.MousePosition.Y - maximizeDragStart.Y);

                if (dx > UNMAX_DRAG_THRESHOLD || dy > UNMAX_DRAG_THRESHOLD)
                {
                    maximizeMouseTarget = Input.MousePosition;
                    maximizeUseMouseTarget = true;
                    Maximized = false;
                    maximizeDragArmed = false; // consume
                }
            }

            // Reset if mouse released before drag threshold
            if (Input.IsUp(MouseButton.Left))
                maximizeDragArmed = false;
        }
        else
        {
            drawMaximizeDragBar = true;
        }

        base.HandleContainerDragging();
    }

    private void DrawCloseButton(Rectangle closeRect, int buttonSize, float iconInsetRatio, Color bg)
    {
        Raylib.DrawRectangleRec(closeRect, bg);

        float inset = buttonSize * iconInsetRatio;
        var a = new Vector2(closeRect.X + inset, closeRect.Y + inset);
        var b = new Vector2(closeRect.X + closeRect.Width - inset, closeRect.Y + closeRect.Height - inset);
        var c = new Vector2(closeRect.X + closeRect.Width - inset, closeRect.Y + inset);
        var d = new Vector2(closeRect.X + inset, closeRect.Y + closeRect.Height - inset);
        Raylib.DrawLineEx(a, b, MathF.Max(1f, buttonSize * 0.08f), Style.TitleBarTextColor);
        Raylib.DrawLineEx(c, d, MathF.Max(1f, buttonSize * 0.08f), Style.TitleBarTextColor);
    }

    private void DrawMaximizeButton(Rectangle maximizeRect, int buttonSize, float iconInsetRatio, Color bg)
    {
        float cornerPadding = 4f;

        Raylib.DrawRectangleRec(maximizeRect, bg);

        float inset = buttonSize * iconInsetRatio;
        float thickness = MathF.Max(2f, buttonSize * 0.08f);

        // Inner square when maximizeProgress = 0
        var inner = new Rectangle(
            maximizeRect.X + inset,
            maximizeRect.Y + inset,
            maximizeRect.Width - inset * 2f,
            maximizeRect.Height - inset * 2f
        );

        // How far corners can travel (clamped so they never go outside padded edge)
        float moveX = MathF.Max(0f, (maximizeRect.Width - inner.Width) / 2f - cornerPadding);
        float moveY = MathF.Max(0f, (maximizeRect.Height - inner.Height) / 2f - cornerPadding);

        // Corner positions shift outward based on maximizeProgress
        float tlX = inner.X - moveX * maximizeProgress;
        float tlY = inner.Y - moveY * maximizeProgress;

        float trX = inner.X + inner.Width + moveX * maximizeProgress;
        float trY = inner.Y - moveY * maximizeProgress;

        float blX = inner.X - moveX * maximizeProgress;
        float blY = inner.Y + inner.Height + moveY * maximizeProgress;

        float brX = inner.X + inner.Width + moveX * maximizeProgress;
        float brY = inner.Y + inner.Height + moveY * maximizeProgress;

        // Interpolate lengths from full side (closed square) to small corner pieces
        float horizontalLen = inner.Width * 0.5f; // was being lerped before
        float verticalLen = inner.Height * 0.5f; // was being lerped before

        Color color = Style.TitleBarTextColor;

        // Top-left
        Raylib.DrawRectangleRec(new Rectangle(tlX, tlY, horizontalLen, thickness), color);
        Raylib.DrawRectangleRec(new Rectangle(tlX, tlY, thickness, verticalLen), color);

        // Top-right
        Raylib.DrawRectangleRec(new Rectangle(trX - horizontalLen, trY, horizontalLen, thickness), color);
        Raylib.DrawRectangleRec(new Rectangle(trX - thickness, trY, thickness, verticalLen), color);

        // Bottom-left
        Raylib.DrawRectangleRec(new Rectangle(blX, blY - thickness, horizontalLen, thickness), color);
        Raylib.DrawRectangleRec(new Rectangle(blX, blY - verticalLen, thickness, verticalLen), color);

        // Bottom-right
        Raylib.DrawRectangleRec(new Rectangle(brX - horizontalLen, brY - thickness, horizontalLen, thickness), color);
        Raylib.DrawRectangleRec(new Rectangle(brX - thickness, brY - verticalLen, thickness, verticalLen), color);
    }

    private void DrawCollapseButton(Rectangle collapseRect, int buttonSize, float iconInsetRatio, Color bg)
    {
        Raylib.DrawRectangleRec(collapseRect, bg);

        float inset = buttonSize * iconInsetRatio;
        float thickness = MathF.Max(1f, buttonSize * 0.1f);

        // centerline
        float centerY = collapseRect.Y + collapseRect.Height / 2f;
        float x1 = collapseRect.X + inset;
        float x2 = collapseRect.X + collapseRect.Width - inset;

        float halfHeight = buttonSize * 0.25f;
        float offset = (0.5f - collapseProgress) * 2f * halfHeight;

        // draw V or ^ shape depending on collapseprogress
        Vector2 left = new Vector2(x1, centerY - offset);
        Vector2 mid = new Vector2((x1 + x2) / 2f, centerY + offset);
        Vector2 right = new Vector2(x2, centerY - offset);

        Raylib.DrawLineEx(left, mid, thickness, Style.TitleBarTextColor);
        Raylib.DrawLineEx(mid, right, thickness, Style.TitleBarTextColor);
    }

    private float ComputeAndApplyBestFontSize(Rectangle titlebarBounds, float maxTextWidth, float maxFontSize, float minFontSize)
    {
        float chosenFontSize = maxFontSize;

        try
        {
            // Binary-search for the largest font size that fits within maxTextWidth and titlebar height.
            float low = minFontSize;
            float high = maxFontSize;
            float best = minFontSize;

            for (int iter = 0; iter < 10; iter++) // 10 iterations gives sub-pixel precision and is very cheap
            {
                float mid = (low + high) / 2f;
                Title.FontSize = (int)mid;

                // CalculateBounds should return a Rectangle-like struct (width/height)
                var bounds = Title.CalculateBounds(maxTextWidth);
                float measuredWidth = bounds.Width;
                float measuredHeight = bounds.Height;

                // ensure text also fits vertically inside the titlebar (small margin)
                bool fitsHorizontally = measuredWidth <= maxTextWidth;
                bool fitsVertically = measuredHeight <= titlebarBounds.Height * 0.9f;

                if (fitsHorizontally && fitsVertically)
                {
                    best = mid;
                    low = mid; // try larger
                }
                else
                {
                    high = mid; // too big, try smaller
                }
            }

            // apply best-found size (clamp to bounds just in case)
            Title.FontSize = (int)MathF.Max(minFontSize, MathF.Min(maxFontSize, best));
            chosenFontSize = Title.FontSize;
        }
        catch
        {
            // fallback: if anything goes wrong, leave chosenFontSize as-is
        }

        return chosenFontSize;
    }

    private void CollapseImmediately()
    {
        // mark collapsed state and targets
        isCollapsed = true;
        collapseTarget = 1f;
        collapseProgress = 1f;
        collapseAnimationFinished = true;

        // set collapsed content alpha (match your collapsed visual)
        Style.ContentAlpha = Math.Max(originalContentAlpha * 0.5f, 1);

        // ensure fullHeightCache is available
        if (fullPosCache.Height <= 0f) fullPosCache.Height = Size.Y;

        // compute collapsed height and apply immediately
        float collapsedHeight = Style.TitleBarHeight;
        TargetSize = CurrentPosition.Size;
        SetSize(new Vector2(CurrentPosition.Size.X, Lerp(fullPosCache.Height, collapsedHeight, 1f)));

    }

    private void StartCloseAnimation()
    {
        if (closeAnimationActive || IsFullyClosed) return;

        closeAnimationActive = true;
        closeProgress = 0f;

        // capture visuals to animate from
        closeStartSize = CurrentPosition.Size;
        closeStartContentAlpha = Style.ContentAlpha;

        hasBeenShown = false;

        // ensure nothing else moves us while closing
        NoAutoMove = true;
    }

    protected override void Update()
    {
        if (openAnimationActive)
        {
            openProgress += Time.deltaTime / Math.Max(0.0001f, OPEN_DURATION);
            openProgress = Math.Clamp(openProgress, 0f, 1f);

            float t = Curves.SlowFastSlow?.Evaluate(openProgress) ?? openProgress;

            float collapsedAlpha = originalContentAlpha * 0.5f;
            Style.ContentAlpha = Lerp(0f, collapsedAlpha, t);

            if (openProgress >= 1f - 0.0001f)
            {
                openAnimationActive = false;

                Style.ContentAlpha = collapsedAlpha;

                if (showMode is ShowMode.Standard)
                    Collapsed = false;
                else if (showMode is ShowMode.Maximized)
                    Maximized = true;

                NoAutoMove = false;
            }
            else
                return;
        }

        float easedForHit = Curves.Linear?.Evaluate(collapseProgress) ?? collapseProgress;
        float collapsedHeight = /*UIConstants.CONTENT_PADDING * 2f +*/ (Style.AllowUserResizing ? Style.TitleBarHeight : 0f);
        float animatedHeightForHit = Lerp(Maximized ? CurrentPosition.Height : fullPosCache.Height, collapsedHeight, easedForHit);

        var hitRect = new Rectangle(CurrentPosition.X, CurrentPosition.Y, CurrentPosition.Width,
                                    collapseProgress == 0f ? animatedHeightForHit : CurrentPosition.Height);

        Input.RequestMouseFocusIfHovered(hitRect);

        if (fullPosCache.Height <= 0f)
            fullPosCache.Height = Size.Y;

        if (collapseProgress != collapseTarget)
        {
            float dir = collapseTarget > collapseProgress ? 1f : -1f;
            collapseProgress += dir * (Time.deltaTime / Math.Max(0.0001f, COLLAPSE_DURATION));
            collapseProgress = Math.Clamp(collapseProgress, 0f, 1f);

            Style.ContentAlpha = Lerp(originalContentAlpha, originalContentAlpha * 0.5f, collapseProgress);
        }

        bool collapseFinished = Math.Abs(collapseProgress - collapseTarget) < 0.0001f;
        if (collapseFinished)
        {
            if (collapseProgress <= 0f)
            {
                if (pendingAction == PendingAction.MaximizeAfterExpand)
                {
                    pendingAction = PendingAction.None;
                    Maximized = true;
                }
            }
            else if (collapseProgress >= 1f)
            {
                if (closeAfterCollapse)
                {
                    closeAfterCollapse = false;
                    StartCloseAnimation();
                }
            }
        }

        if (maximizeAnimationActive)
        {
            // step progress toward target
            float dir = maximizeTarget > maximizeProgress ? 1f : -1f;
            maximizeProgress += dir * (Time.deltaTime / Math.Max(0.0001f, MAXIMIZE_DURATION));
            maximizeProgress = Math.Clamp(maximizeProgress, 0f, 1f);

            // eased t for nicer motion
            float eased = Curves.SlowFastSlow?.Evaluate(maximizeProgress) ?? maximizeProgress;
            if (dir is -1)
                eased = 1 - eased;
            // interpolate from captured start -> captured end
            SetPosition(LerpVec2(maximizeAnimStartPos, maximizeAnimEndPos, eased));
            SetSize(LerpVec2(maximizeAnimStartSize, maximizeAnimEndSize, eased));

            // keep visual and targets in sync during animation so other reads see expected values
            TargetPosition = CurrentPosition.Position;
            TargetSize = CurrentPosition.Size;

            // finalize when animation reaches the requested target
            if (Math.Abs(maximizeProgress - maximizeTarget) < 0.0001f)
            {
                maximizeAnimationActive = false;

                // finalize precisely to the captured end geometry
                SetPosition(maximizeAnimEndPos);
                SetSize(maximizeAnimEndSize);
                TargetPosition = maximizeAnimEndPos;
                TargetSize = maximizeAnimEndSize;

                // now update the actual maximized state
                isMaximized = maximizeFinalizeTargetIsMaximized;

                // re-evaluate NoAutoMove: keep it true when fully maximized, otherwise restore movement
                NoAutoMove = isMaximized;

                if (maximizeRequested)
                {
                    bool queued = maximizeRequestedValue;
                    maximizeRequested = false;
                    maximizeRequestedValue = false;
                    Maximized = queued;
                }
            }
        }

        if (!isMaximized && pendingAction == PendingAction.CollapseAfterUnmaximize)
        {
            pendingAction = PendingAction.None;
            Collapsed = true; // starts collapse animation
        }

        base.Update();

        if (collapseProgress <= 0f)
        {
            Style.RaiseOnHover = false;
        }
    }

    protected internal override void Draw()
    {
        // we place this in Draw instead of update, because when the dialog is closing Update is no longer called
        if (closeAnimationActive)
        {
            closeProgress += Time.deltaTime / Math.Max(0.0001f, CLOSE_DURATION);
            closeProgress = Math.Clamp(closeProgress, 0f, 1f);

            // easing — pick a curve that feels good (Linear used as safe default)
            float t = Curves.SlowFastSlow?.Evaluate(closeProgress) ?? closeProgress;

            // shrink height to zero while keeping width (you can Lerp both if you want)
            var newSize = new Vector2(closeStartSize.X, Lerp(closeStartSize.Y, 0f, t));
            SetSize(newSize);
            TargetSize = newSize;

            // fade out content alpha completely
            Style.ContentAlpha = Lerp(closeStartContentAlpha, 0f, t);

            // optional: move origin so shrink looks anchored to top-left (keeps top steady)
            // keep CurrentPosition.X/Y unchanged so it visually collapses downward;
            // if you want it to collapse centered, adjust position here.

            if (closeProgress >= 1f - 0.0001f)
            {
                closeAnimationActive = false;
                IsFullyClosed = true;
                IsClosing = false; // set to false so base can handle propper closing of the rest too
                base.Close();
            }
            Input.IsRequestingKeyboardFocus = Input.IsRequestingMouseFocus = false;
            return;
        }

        float hoverOffsetX = 0f, hoverOffsetY = 0f;
        float shadowOffsetX = 0f, shadowOffsetY = 0f;

        if (!isCollapsed && collapseProgress <= 0f)
        {
            DrawMaximizeDragBar();
            Style.RaiseOnHover = false;
            collapseAnimationFinished = true;
            base.Draw();
            return;
        }

        if (isCollapsed)
        {
            if (collapseProgress == 1)
            {
                if (!collapseAnimationFinished)
                {
                    Style.RaiseOnHover = true;
                    if (IsHovered())
                    {
                        Style.StyleBase.currentRaiseAmount = 0f;
                    }
                    else
                    {
                        Style.StyleBase.currentRaiseAmount = 1;
                        isHoverTarget = false;
                    }
                }

                collapseAnimationFinished = true;
                HandleRaiseAnimation(ref hoverOffsetX, ref hoverOffsetY, ref shadowOffsetX, ref shadowOffsetY);
            }
        }

        {
            float eased = Curves.SlowFastSlow?.Evaluate(collapseProgress) ?? collapseProgress;
            float collapsedHeight = Style.TitleBarHeight;
            float animatedHeight = Lerp(fullPosCache.Height, collapsedHeight, eased);
            SetSize(new(Size.X, animatedHeight));

            var backgroundBounds = new Rectangle(
                CurrentPosition.X + hoverOffsetX,
                CurrentPosition.Y + hoverOffsetY,
                CurrentPosition.Width,
                animatedHeight);

            if (isCollapsed && collapseProgress == 1f)
            {
                float amount = Style.StyleBase.currentRaiseAmount * Style.HoverRaiseAmount;
                var shadowRect = new Rectangle(
                    backgroundBounds.X - (Style.ShadowSizeLeft - shadowOffsetX) * amount,
                    backgroundBounds.Y - (Style.ShadowSizeTop - shadowOffsetY) * amount,
                    backgroundBounds.Width + (Style.ShadowSizeLeft + Style.ShadowSizeRight),
                    backgroundBounds.Height + (Style.ShadowSizeTop + Style.ShadowSizeBottom)
                );
                ray.DrawRectangleRec(shadowRect, Style.Shadow);
            }

            // background, border, titlebar
            ray.DrawRectangleRec(backgroundBounds, Style.Background);
            ray.DrawRectangleLinesEx(backgroundBounds, Style.BorderSize, Style.Border);

            // draw titlebar using same rect math as base
            Rectangle dragBounds = new Rectangle(
                     backgroundBounds.X + Style.BorderSize,
                     backgroundBounds.Y + Style.BorderSize,
                     backgroundBounds.Width - Style.BorderSize * 2,
                     Style.TitleBarHeight - Style.BorderSize);

            HandleTitleBar(backgroundBounds, dragBounds);

            // compute content area based on animated height (bottom is always background - padding)
            float contentY = backgroundBounds.Y + UIConstants.CONTENT_PADDING + (Style.AllowUserResizing ? Style.TitleBarHeight : 0f);
            float contentHeight = backgroundBounds.Y + backgroundBounds.Height - UIConstants.CONTENT_PADDING - contentY;
            float contentWidth = backgroundBounds.Width - UIConstants.CONTENT_PADDING * 2;

            // if scrollbar visible, reserve animated width (use existing ScrollbarCurrentWidth)
            if (IsScrollbarVisible)
                contentWidth = Math.Max(0f, contentWidth - (ScrollbarCurrentWidth + UIConstants.CONTENT_PADDING));

            Rectangle contentArea = new Rectangle(
                backgroundBounds.X + UIConstants.CONTENT_PADDING,
                contentY,
                contentWidth,
                contentHeight
            );

            // store for input/layout systems
            LastContentRenderBounds = contentArea;
            LastBorderBounds = backgroundBounds;

            if (contentArea.Height <= 0)
                return;

            // draw scissored content (only the portion that fits in the animated area)
            DrawContent(contentArea);

            // draw close timer if present (same as base)
            if (Style.TimeUntilAutoDismiss > 0)
                DrawCloseTimerBar(backgroundBounds);
        }
    }

    private float maximizeBarSize = 5;
    private float maximizeBarTopMargin = 0;
    private float maximizeBarSideMargin = 0;
    private float maximizeBarAnimation = 0; // 0 - 1
    private Color maximizeBarColor = Color.Gray.WithAlpha(120);

    private float maximizeBarBorderThickness = 2f;
    private Color maximizeBarBorderColor = Color.White;
    private bool maximizeBarHovered = false;
    private const float MAXIMIZE_BAR_ANIM_SPEED = 8f;

    private float maximizeBarVisibility = 0f;   // 0..1
    private float maximizeBarExpansion = 0f;   // 0..1
    private const float MAXIMIZE_BAR_VIS_SPEED = 8f;
    private const float MAXIMIZE_BAR_EXP_SPEED = 12f;


    private void DrawMaximizeDragBar()
    {
        var winSize = ForgeWardenEngine.Current.Window.Size;

        var baseRect = new Rectangle(
            maximizeBarSideMargin,
            maximizeBarTopMargin,
            MathF.Max(0f, winSize.X - maximizeBarSideMargin * 2f),
            maximizeBarSize
        );

        var expandedRect = new Rectangle(0, 0, winSize.X, winSize.Y);

        var mouse = Input.MousePosition;

        bool hoverBase = Raylib.CheckCollisionPointRec(mouse, baseRect);

        if (hoverBase && Input.IsUp(MouseButton.Left))
        {
            Maximized = true;
        }

        float visTarget = IsBeingDragged ? 1f : 0f;
        float expTarget = (IsBeingDragged && hoverBase) ? 1f : 0f;

        {
            float delta = Time.deltaTime * MAXIMIZE_BAR_VIS_SPEED;
            if (maximizeBarVisibility < visTarget)
                maximizeBarVisibility = MathF.Min(visTarget, maximizeBarVisibility + delta);
            else if (maximizeBarVisibility > visTarget)
                maximizeBarVisibility = MathF.Max(visTarget, maximizeBarVisibility - delta);
        }

        {
            float delta = Time.deltaTime * MAXIMIZE_BAR_EXP_SPEED;
            if (maximizeBarExpansion < expTarget)
                maximizeBarExpansion = MathF.Min(expTarget, maximizeBarExpansion + delta);
            else if (maximizeBarExpansion > expTarget)
                maximizeBarExpansion = MathF.Max(expTarget, maximizeBarExpansion - delta);
        }

        if (maximizeBarVisibility <= 0.001f)
            return;

        float combinedT = maximizeBarVisibility * maximizeBarExpansion;
        Rectangle currentRect = LerpRect(baseRect, expandedRect, combinedT);

        byte baseAlpha = 128;

        ray.DrawRectangleRec(currentRect, maximizeBarColor);

        byte borderBaseAlpha = 64;
        byte borderFullAlpha = 255;
        float borderAlphaF = borderBaseAlpha + (borderFullAlpha - borderBaseAlpha) * (maximizeBarVisibility * maximizeBarExpansion);
        byte borderAlpha = (byte)Math.Clamp((int)borderAlphaF, 0, 255);
        var borderColor = new Color(maximizeBarBorderColor.R, maximizeBarBorderColor.G, maximizeBarBorderColor.B, borderAlpha);
        ray.DrawRectangleLinesEx(currentRect, maximizeBarBorderThickness, borderColor);

        if (maximizeBarExpansion < 0.95f)
        {
            float handleW = MathF.Min(120f, currentRect.Width * 0.5f);
            float handleH = MathF.Max(4f, currentRect.Height * 0.12f);
            var handleRect = new Rectangle(
                currentRect.X + (currentRect.Width - handleW) / 2f,
                currentRect.Y + (currentRect.Height - handleH) / 2f,
                handleW,
                handleH
            );

            ray.DrawRectangleRec(handleRect, maximizeBarColor);
            ray.DrawRectangleLinesEx(handleRect, 1f, maximizeBarBorderColor);
        }

        static Rectangle LerpRect(Rectangle a, Rectangle b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            float x = Lerp(a.X, b.X, t);
            float y = Lerp(a.Y, b.Y, t);
            float w = Lerp(a.Width, b.Width, t);
            float h = Lerp(a.Height, b.Height, t);
            return new Rectangle(x, y, w, h);
        }
    }

    public override void Close()
    {
        if (IsClosed) return;

        if (!isMaximized)
            fullPosCache.Position = CurrentPosition.Position;

        IsClosed = true;
        if (isMaximized || maximizeAnimationActive || (maximizeProgress > 0f && maximizeTarget > 0f))
        {
            pendingAction = PendingAction.CollapseAfterUnmaximize;
            closeAfterCollapse = true;
            Maximized = false;
            return;
        }

        if (collapseProgress < 1f || (collapseTarget > 0f && collapseProgress < collapseTarget))
        {
            closeAfterCollapse = true;
            Collapsed = true;
            return;
        }

        StartCloseAnimation();
        base.Close();
        OnClosing.Invoke(this);
    }

    protected static Vector2 LerpVec2(Vector2 a, Vector2 b, float t)
    {
        return new Vector2(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));
    }
}
