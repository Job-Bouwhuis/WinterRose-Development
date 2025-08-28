using Raylib_cs;
using System;
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

    private PendingAction pendingAction = PendingAction.None;

    private InputContext input = new(new RaylibInputProvider(), -10000, true);
    private RichText title;

    private float collapseProgress = 0f;      // 0 = full, 1 = collapsed
    private float collapseTarget = 0f;
    private const float COLLAPSE_DURATION = 0.22f;

    private float maximizeProgress = 0f;      // 0 = normal size, 1 = normal maximized
    private float maximizeTarget = 0f;
    private const float MAXIMIZE_DURATION = 0.22f;

    private float originalContentAlpha = 1f;
    private Rectangle fullPosCache;

    // closing state
    public bool IsClosed { get; private set; } = false;
    /// <summary>
    /// Set by the window when the full close animation has been completed, and should be removed from the list of windows
    /// </summary>
    internal bool IsFullyClosed { get; private set; } = false;

    private bool closeAfterCollapse = false;
    private bool closeAnimationActive = false;
    private float closeProgress = 0f;
    private const float CLOSE_DURATION = 0.18f;

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
            // If an animation is already running, queue the request and return.
            if (maximizeAnimationActive)
            {
                maximizeRequested = true;
                maximizeRequestedValue = value;
                return;
            }

            // If already in the requested state, nothing to do.
            if (isMaximized == value) return;

            // Record what the animation should finalize to (do NOT flip isMaximized yet)
            maximizeFinalizeTargetIsMaximized = value;

            // Prepare animation targets
            maximizeTarget = value ? 1f : 0f;
            maximizeAnimStartPos = CurrentPosition.Position;
            maximizeAnimStartSize = CurrentPosition.Size;

            if (value) // starting maximize
            {
                // snapshot current geometry for restore later
                positionBeforeMaximize = maximizeAnimStartPos;
                sizeBeforeMaximize = maximizeAnimStartSize;

                maximizeAnimEndPos = new Vector2(0f, 0f);
                maximizeAnimEndSize = new Vector2(Application.Current.Window.Width, Application.Current.Window.Height);

                NoAutoMove = true;
            }
            else // starting un-maximize
            {
                maximizeAnimEndPos = positionBeforeMaximize;
                maximizeAnimEndSize = sizeBeforeMaximize;

                if (maximizeAnimEndSize.X == 0f && maximizeAnimEndSize.Y == 0f)
                {
                    // fallback if we somehow have no stored pre-maximize geometry
                    maximizeAnimEndPos = maximizeAnimStartPos;
                    maximizeAnimEndSize = maximizeAnimStartSize;
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
            CurrentPosition.Position = TargetPosition = value;
            Console.WriteLine($"[UIWindow] CurrentPosition.Position changed: {CurrentPosition.Position}");
            if (CurrentPosition.Position.Y < 0f) System.Diagnostics.Debugger.Break();
        }
    }

    public void ResetState()
    {
        collapseProgress = 0f;
        collapseTarget = 0f;

        maximizeProgress = 0f;
        maximizeTarget = 0f;

        IsClosed = false;
        IsFullyClosed = false; Console.WriteLine($"[UIWindow] CurrentPosition.Position changed (maximizing anim): {CurrentPosition.Position}");
        if (CurrentPosition.Position.Y < 0f) System.Diagnostics.Debugger.Break(); 
        IsClosing = false;

        CurrentPosition = fullPosCache;
        TargetPosition = fullPosCache.Position;
        TargetSize = fullPosCache.Size;
        Console.WriteLine($"[UIWindow] CurrentPosition.Position changed (ResetState): {CurrentPosition.Position}");
        if (CurrentPosition.Position.Y < 0f) System.Diagnostics.Debugger.Break();

        closeAfterCollapse = false;
        closeAnimationActive = false;
        closeProgress = 0f;

        hasBeenShown = false;
        openAnimationActive = false;
        openProgress = 0f;

        NoAutoMove = false;
    }

    public Vector2 Size
    {
        get => CurrentPosition.Size;
        set => CurrentPosition.Size = TargetSize = value;
    }

    public new WindowStyle Style
    {
        get => (WindowStyle)base.Style;
        set => base.Style = value;
    }

    private bool collapseAnimationFinished;

    public override float Height => CurrentPosition.Size.Y + base.Height;

    public RichText Title
    {
        get => title;
        set => title = value ?? "";
    }

    public UIWindow(RichText title, float width, float height) : this(title, width, height, 10, 10) { }

    public UIWindow(RichText title, float width, float height, float x, float y)
    {
        Style = new WindowStyle();
        Size = new Vector2(width, height);
        Position = new(x, y);
        Title = title;

        originalContentAlpha = Style.ContentAlpha;
        fullPosCache = new(x, y, width, height);
    }

    public void ToggleCollapsed()
    {
        // If maximized (or animating toward maximizing), unmaximize first then collapse
        bool currentlyMaximized = isMaximized || maximizeAnimationActive || (maximizeProgress > 0f && maximizeTarget > 0f);
        if (currentlyMaximized)
        {
            pendingAction = PendingAction.CollapseAfterUnmaximize;
            Maximized = false; // start unmaximize animation
            return;
        }

        // otherwise toggle collapsed normally
        Collapsed = !isCollapsed;
    }

    public void ToggleMaximized()
    {
        // If collapsed (or animating toward collapsed), uncollapse first then maximize
        bool currentlyCollapsed = isCollapsed || (collapseProgress > 0f && collapseTarget > 0f);
        if (currentlyCollapsed)
        {
            pendingAction = PendingAction.MaximizeAfterExpand;
            Collapsed = false; // start uncollapse animation
            return;
        }

        // otherwise just toggle maximize normally
        Maximized = !isMaximized;
    }

    public void Show()
    {
        WindowManager.Show(this);
        ResetState();

        if (!hasBeenShown)
        {
            hasBeenShown = true;
            StartOpenSequence();
        }
    }

    private void StartOpenSequence()
    {
        // guard
        if (openAnimationActive || IsFullyClosed) return;

        // ensure flags are in the right state
        IsClosed = false;
        IsFullyClosed = false;
        closeAfterCollapse = false;
        closeAnimationActive = false;

        CollapseImmediately();

        //// force collapsed visual state (fully collapsed)
        //isCollapsed = true;
        //collapseTarget = 1f;
        //collapseProgress = 1f;

        // compute collapsed height immediately (same math you use elsewhere)
        //float collapsedHeight = Style.TitleBarHeight;
        //CurrentPosition.Size = new Vector2(CurrentPosition.Size.X, Lerp(fullHeightCache, collapsedHeight, 1f));
        //TargetSize = CurrentPosition.Size;

        // start fully transparent (we'll fade into the collapsed appearance)
        Style.ContentAlpha = 0f;

        // kick off open animation (fade-in), which on completion will uncollapse
        openAnimationActive = true;
        openProgress = 0f;

        // prevent other movement while doing the entrance
        NoAutoMove = true;
    }

    public override bool IsHovered()
    {
        if (collapseProgress <= 0f)
            return base.IsHovered();

        // use the same easing/height as Update() so hover semantics match the visual collapsed height
        float eased = Curves.EaseOutBackFar?.Evaluate(collapseProgress) ?? collapseProgress;
        float collapsedHeight = UIConstants.CONTENT_PADDING * 2f + (Style.AllowDragging ? Style.TitleBarHeight : 0f);
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
        float maxTextWidth = collapseRect.X - leftPadding - 8; // small extra spacing
        if (maxTextWidth < 8) maxTextWidth = 8;

        float maxFontSize = titlebarBounds.Height * 0.6f;
        float minFontSize = 8f;

        float chosenFontSize = ComputeAndApplyBestFontSize(titlebarBounds, maxTextWidth, maxFontSize, minFontSize);

        // draw the rich text; align vertically centered
        var textPos = new Vector2(leftPadding, titlebarBounds.Y + (titlebarBounds.Height - (int)(chosenFontSize)) / 2f - 1f);
        RichTextRenderer.DrawRichText(Title, textPos, maxTextWidth, Style.White, null);

        // --- interaction: if any button was released while mouse was over it AND this window has mouse focus,
        // return false so the base dragging logic won't run. (Caller can handle the actual action elsewhere.) ---
        if (windowHasMouseFocus && mouseReleased)
        {
            if (overClose || overMaximize || overCollapse)
            {
                // perform collapse immediately when collapse button clicked
                if (overCollapse)
                    ToggleCollapsed();
                if (overMaximize)
                    ToggleMaximized();
                if (overClose)
                    Close();

                // a button was activated — prevent propagation to dragging
                return false;
            }
        }

        // no custom interaction — allow normal dragging behavior
        return true;
    }

    private void ComputeButtonRects(Rectangle titlebarBounds, float buttonSizeRatio, int buttonPadding, int buttonSpacing,
                                           out int buttonSize, out Rectangle closeRect, out Rectangle maximizeRect, out Rectangle collapseRect, out float right)
    {
        float buttonSizeF = titlebarBounds.Height * buttonSizeRatio;
        buttonSize = (int)MathF.Max(12, buttonSizeF); // keep a sensible minimum

        right = titlebarBounds.X + titlebarBounds.Width - buttonPadding;
        closeRect = new Rectangle(right - buttonSize, titlebarBounds.Y + (titlebarBounds.Height - buttonSize) / 2f, buttonSize, buttonSize);
        right -= (buttonSize + buttonSpacing);
        maximizeRect = new Rectangle(right - buttonSize, titlebarBounds.Y + (titlebarBounds.Height - buttonSize) / 2f, buttonSize, buttonSize);
        right -= (buttonSize + buttonSpacing);
        collapseRect = new Rectangle(right - buttonSize, titlebarBounds.Y + (titlebarBounds.Height - buttonSize) / 2f, buttonSize, buttonSize);
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
        Raylib.DrawRectangleRec(maximizeRect, bg);

        float inset = buttonSize * iconInsetRatio;
        var r = new Rectangle(maximizeRect.X + inset, maximizeRect.Y + inset, maximizeRect.Width - inset * 2f, maximizeRect.Height - inset * 2f);
        Raylib.DrawRectangleLinesEx(r, MathF.Max(1f, buttonSize * 0.06f), Style.TitleBarTextColor);
    }

    private void DrawCollapseButton(Rectangle collapseRect, int buttonSize, float iconInsetRatio, Color bg)
    {
        Raylib.DrawRectangleRec(collapseRect, bg);

        float inset = buttonSize * iconInsetRatio;
        var y = collapseRect.Y + collapseRect.Height / 2f;
        var x1 = collapseRect.X + inset;
        var x2 = collapseRect.X + collapseRect.Width - inset;
        Raylib.DrawLineEx(new Vector2(x1, y), new Vector2(x2, y), MathF.Max(1f, buttonSize * 0.08f), Style.TitleBarTextColor);
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
        float collapsedHeight = (Style.AllowDragging ? Style.TitleBarHeight : 0f);
        TargetSize = CurrentPosition.Size;
        CurrentPosition.Size = new Vector2(CurrentPosition.Size.X, Lerp(fullPosCache.Height, collapsedHeight, 1f));

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
        Console.WriteLine(CurrentPosition);
        if (openAnimationActive)
        {
            openProgress += Time.deltaTime / Math.Max(0.0001f, OPEN_DURATION);
            openProgress = Math.Clamp(openProgress, 0f, 1f);

            // two-phase feel: fade while collapsed, then trigger the uncollapse
            // here we use the whole OPEN_DURATION to fade — when done, start the normal uncollapse animation
            float t = Curves.SlowFastSlow?.Evaluate(openProgress) ?? openProgress;

            // target collapsed alpha (matches your collapsed visual: original * 0.5)
            float collapsedAlpha = originalContentAlpha * 0.5f;
            Style.ContentAlpha = Lerp(0f, collapsedAlpha, t);

            // when fade completes, start uncollapse and finish the open animation
            if (openProgress >= 1f - 0.0001f)
            {
                openAnimationActive = false;

                // ensure collapsed alpha is exactly the intended value
                Style.ContentAlpha = collapsedAlpha;

                // now uncollapse using your normal code path (this will animate collapseProgress from 1 -> 0)
                Collapsed = false;

                // allow movement systems again (they may be needed while uncollapsing)
                NoAutoMove = false;
            }
            else
                // we keep returning early while the entrance fade runs so no other input/movement interferes
                return;
        }

        float easedForHit = Curves.Linear?.Evaluate(collapseProgress) ?? collapseProgress;
        float collapsedHeight = /*UIConstants.CONTENT_PADDING * 2f +*/ (Style.AllowDragging ? Style.TitleBarHeight : 0f);
        float animatedHeightForHit = Lerp(fullPosCache.Height, collapsedHeight, easedForHit);

        var hitRect = new Rectangle(CurrentPosition.X, CurrentPosition.Y, CurrentPosition.Width,
                                    collapseProgress == 0f ? animatedHeightForHit : CurrentPosition.Height);

        if (closeAnimationActive)
        {
            closeProgress += Time.deltaTime / Math.Max(0.0001f, CLOSE_DURATION);
            closeProgress = Math.Clamp(closeProgress, 0f, 1f);

            // easing — pick a curve that feels good (Linear used as safe default)
            float t = Curves.SlowFastSlow?.Evaluate(closeProgress) ?? closeProgress;

            // shrink height to zero while keeping width (you can Lerp both if you want)
            var newSize = new Vector2(closeStartSize.X, Lerp(closeStartSize.Y, 0f, t));
            CurrentPosition.Size = newSize;
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

                // leave IsClosed = true (request was already submitted)
                // keep NoAutoMove true (we're closed)
                // if you want the base class notified, set IsClosing so any other logic knows
                IsClosing = true;
            }
            Input.IsRequestingKeyboardFocus = Input.IsRequestingMouseFocus = false;
            return;
        }


        Input.RequestMouseFocusIfHovered(hitRect);

        if (fullPosCache.Height <= 0f)
            fullPosCache.Height = Size.Y;

        if (collapseProgress != collapseTarget)
        {
            float dir = collapseTarget > collapseProgress ? 1f : -1f;
            collapseProgress += dir * (Time.deltaTime / Math.Max(0.0001f, COLLAPSE_DURATION));
            collapseProgress = Math.Clamp(collapseProgress, 0f, 1f);

            Style.ContentAlpha = Lerp(originalContentAlpha, originalContentAlpha * 0.5f, collapseProgress);
            //CurrentPosition.Height = Lerp(fullHeightCache, CurrentPosition.Height, collapseProgress);
        }

        bool collapseFinished = Math.Abs(collapseProgress - collapseTarget) < 0.0001f;
        if (collapseFinished)
        {
            // just finished expanding (becoming normal size)
            if (collapseProgress <= 0f)
            {
                if (pendingAction == PendingAction.MaximizeAfterExpand)
                {
                    pendingAction = PendingAction.None;
                    Maximized = true;
                }
            }
            // just finished collapsing (becoming minimized)
            else if (collapseProgress >= 1f)
            {
                // if we queued a maximize-after-expand earlier this won't trigger here;
                // but if we wanted to close after collapse, start the close animation now
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
            CurrentPosition.Position = LerpVec2(maximizeAnimStartPos, maximizeAnimEndPos, eased);
            CurrentPosition.Size = LerpVec2(maximizeAnimStartSize, maximizeAnimEndSize, eased);

            Console.WriteLine($"[UIWindow] CurrentPosition.Position changed (maximizing anim): {CurrentPosition.Position}");
            if (CurrentPosition.Position.Y < 0f) System.Diagnostics.Debugger.Break();

            // keep visual and targets in sync during animation so other reads see expected values
            TargetPosition = CurrentPosition.Position;
            TargetSize = CurrentPosition.Size;
            
            // finalize when animation reaches the requested target
            if (Math.Abs(maximizeProgress - maximizeTarget) < 0.0001f)
            {
                maximizeAnimationActive = false;

                // finalize precisely to the captured end geometry
                CurrentPosition.Position = maximizeAnimEndPos;
                CurrentPosition.Size = maximizeAnimEndSize;
                TargetPosition = maximizeAnimEndPos;
                TargetSize = maximizeAnimEndSize;

                Console.WriteLine($"[UIWindow] CurrentPosition.Position changed (maximizing final): {CurrentPosition.Position}");
                if (CurrentPosition.Position.Y < 0f) System.Diagnostics.Debugger.Break();

                // now update the actual maximized state
                isMaximized = maximizeFinalizeTargetIsMaximized;

                // re-evaluate NoAutoMove: keep it true when fully maximized, otherwise restore movement
                NoAutoMove = isMaximized;

                // if a toggle was queued while animating, apply it now (this will start a new animation)
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
        float hoverOffsetX = 0f, hoverOffsetY = 0f;
        float shadowOffsetX = 0f, shadowOffsetY = 0f;

        if (!isCollapsed && collapseProgress <= 0f)
        {
            Style.RaiseOnHover = false;
            collapseAnimationFinished = false;
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
                        Style.currentRaiseAmount = 0f;
                    }
                    else
                    {
                        Style.currentRaiseAmount = 1;
                        isHoverTarget = false;
                    }
                }
                collapseAnimationFinished = true;
                HandleRaiseAnimation(ref hoverOffsetX, ref hoverOffsetY, ref shadowOffsetX, ref shadowOffsetY);
            }
        }

        {

            // compute animated height (background visually shrinks)
            float eased = Curves.SlowFastSlow?.Evaluate(collapseProgress) ?? collapseProgress;
            float collapsedHeight = /*UIConstants.CONTENT_PADDING * 2f +*/ (Style.AllowDragging ? Style.TitleBarHeight : 0f);
            float animatedHeight = Lerp(fullPosCache.Height, collapsedHeight, eased);

            // Only apply the raise visual to the titlebar when collapsed AND hovered.
            // (Non-collapsed windows don't use raise here.)0

            // background positioned at CurrentPosition (do not mutate CurrentPosition)
            var backgroundBounds = new Rectangle(
                CurrentPosition.X + hoverOffsetX,
                CurrentPosition.Y + hoverOffsetY,
                CurrentPosition.Width,
                animatedHeight);

            if (isCollapsed && collapseProgress == 1f)
            {

                // normal shadow behavior (use the shadow offsets produced by HandleRaiseAnimation)
                float amount = Style.currentRaiseAmount * Style.HoverRaiseAmount;
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
            float contentY = backgroundBounds.Y + UIConstants.CONTENT_PADDING + (Style.AllowDragging ? Style.TitleBarHeight : 0f);
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
            if (TimeUntilAutoDismiss > 0)
                DrawCloseTimerBar(backgroundBounds);
        }
    }

    public override void Close()
    {
        // if already requested, ignore
        if (IsClosed) return;

        fullPosCache.Position = CurrentPosition.Position;

        // mark request immediately
        IsClosed = true;
        // if we're maximized, unmaximize first, then collapse, then close
        if (isMaximized || maximizeAnimationActive || (maximizeProgress > 0f && maximizeTarget > 0f))
        {
            // queue collapse to happen after the un-maximize completes
            pendingAction = PendingAction.CollapseAfterUnmaximize;
            closeAfterCollapse = true;
            Maximized = false; // start unmaximize
            return;
        }

        // if we are not yet fully collapsed, collapse first then close
        if (collapseProgress < 1f || (collapseTarget > 0f && collapseProgress < collapseTarget))
        {
            closeAfterCollapse = true;
            Collapsed = true; // start collapse
            return;
        }

        // already collapsed -> start the final shrink+fade immediately
        StartCloseAnimation();
        base.Close();
    }


    // existing helper area - add this helper
    protected static Vector2 LerpVec2(Vector2 a, Vector2 b, float t)
    {
        return new Vector2(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));
    }
}
