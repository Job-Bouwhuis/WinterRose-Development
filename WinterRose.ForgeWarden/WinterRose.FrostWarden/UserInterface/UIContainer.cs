using PuppeteerSharp;
using Raylib_cs;
using System.Diagnostics;
using System.Threading.Tasks;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.Utility;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace WinterRose.ForgeWarden.UserInterface;

public abstract class UIContainer
{
    // --- Input ---
    public abstract InputContext Input { get; }

    public virtual bool IsClosing { get; protected internal set; }
    public bool IsVisible => !IsClosing;

    public bool IsBeingDragged { get; private set; } = false;
    private float dragHeight => Style.AllowUserResizing ? Style.TitleBarHeight : 0;
    /// <summary>
    /// Used for a preview. when unpaused, the container moves back to the mouse
    /// </summary>
    public bool PauseDragMovement { get; set; }
    private bool prevPauseDragMovement;
    private Rectangle curPos = new Rectangle();
    internal Rectangle CurrentPosition
    {
        get
        {
            return curPos;
        }
        set
        {
            curPos = value;
        }
    }

    protected float ContentScrollY = 0f;
    protected bool IsScrollDragging = false;
    protected float ScrollDragOffset = 0f;
    protected bool IsScrollbarVisible = false;
    protected float LastTotalContentHeight = 0f;

    protected const float SCROLLBAR_COLLAPSED_WIDTH = 8f;
    protected const float SCROLLBAR_EXPANDED_WIDTH = 18f;
    protected const float SCROLLBAR_ANIM_DURATION = 0.12f; // seconds
    protected float ScrollbarAnimProgress = 0f; // 0 = collapsed, 1 = expanded
    protected bool ScrollbarHoverTarget = false;
    protected float ScrollbarCurrentWidth = SCROLLBAR_COLLAPSED_WIDTH;
    protected const float SCROLL_MIN_THUMB = 16f;
    protected const float SCROLL_WHEEL_SPEED = 40f;
    private float scrollbarHoverTimer = 0f;
    private const float SCROLLBAR_HOVER_DELAY = 0.05f; // seconds

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

    private const float RESIZE_MARGIN = 8f;
    private const int RESIZE_DEBUG_ALPHA = 120;
    private const float MIN_WIDTH = 80f;
    private const float MIN_HEIGHT = 40f;

    private bool IsResizing = false;
    private ResizeEdge CurrentResizeEdge = ResizeEdge.None;
    private Vector2 ResizeStartMouse;
    private Rectangle ResizeStartRect;

    internal Vector2 CurrentSize;
    internal Vector2 TargetPosition;
    internal Vector2 TargetSize;
    internal float AnimationElapsed;

    private enum ResizeEdge
    {
        None,
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    protected bool IsDragTarget { get; private set; }

    protected bool isHoverTarget = false;

    private bool PauseAutoDismissTimer { get; set; }
    internal float TimeShown;

    public ContainerStyle Style { get; set; }

    internal List<UIContent> Contents { get; } = new();

    protected internal bool IsMorphDrawing { get; set; }
    protected Rectangle LastContentRenderBounds { get; set; }
    public Rectangle LastBorderBounds { get; protected set; }

    /// <summary>
    /// When true, The container will not attempt to move itself whenever TargetPosition changes. 
    /// <br></br> This can be useful when you want to manage the position of the container manually.
    /// </summary>
    public bool NoAutoMove { get; set; }
    /// <summary>
    /// Draws some extra rectangles and other information
    /// </summary>
    public bool EnableDebugDraw { get; set; }
    protected bool OverrideIsHoveredState { get; set; }

    bool initialized = false;

    internal void UpdateContainer()
    {
        Input.IsRequestingKeyboardFocus = false;
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

        HandleResizing();
        HandleContentUpdates();
        HandleContainerDragging();
        HandleAutoClose();
    }

    protected virtual void HandleResizing()
    {
        // if style doesn't allow dragging/resizing, skip (adjust as needed)
        if (!Style.AllowUserResizing)
            return;

        var mouse = Input.Provider.MousePosition;

        float left = CurrentPosition.X;
        float right = CurrentPosition.X + CurrentPosition.Width;
        float top = CurrentPosition.Y;
        float bottom = CurrentPosition.Y + CurrentPosition.Height;

        bool nearLeft = Math.Abs(mouse.X - left) <= RESIZE_MARGIN && mouse.Y >= top - RESIZE_MARGIN && mouse.Y <= bottom + RESIZE_MARGIN;
        bool nearRight = Math.Abs(mouse.X - right) <= RESIZE_MARGIN && mouse.Y >= top - RESIZE_MARGIN && mouse.Y <= bottom + RESIZE_MARGIN;
        bool nearTop = Math.Abs(mouse.Y - top) <= RESIZE_MARGIN && mouse.X >= left - RESIZE_MARGIN && mouse.X <= right + RESIZE_MARGIN;
        bool nearBottom = Math.Abs(mouse.Y - bottom) <= RESIZE_MARGIN && mouse.X >= left - RESIZE_MARGIN && mouse.X <= right + RESIZE_MARGIN;

        ResizeEdge hoverEdge = ResizeEdge.None;
        if (nearLeft && nearTop) hoverEdge = ResizeEdge.TopLeft;
        else if (nearRight && nearTop) hoverEdge = ResizeEdge.TopRight;
        else if (nearLeft && nearBottom) hoverEdge = ResizeEdge.BottomLeft;
        else if (nearRight && nearBottom) hoverEdge = ResizeEdge.BottomRight;
        else if (nearLeft) hoverEdge = ResizeEdge.Left;
        else if (nearRight) hoverEdge = ResizeEdge.Right;
        else if (nearTop) hoverEdge = ResizeEdge.Top;
        else if (nearBottom) hoverEdge = ResizeEdge.Bottom;

        var lmb = new InputBinding(InputDeviceType.Mouse, (int)MouseButton.Left);

        // start resizing when clicking on an edge
        if (!IsResizing)
        {
            if (hoverEdge != ResizeEdge.None && Input.Provider.IsPressed(lmb))
            {
                Input.IsRequestingMouseFocus = true;
                IsResizing = true;
                CurrentResizeEdge = hoverEdge;
                ResizeStartMouse = mouse;
                ResizeStartRect = CurrentPosition;
                return; // started resizing this frame
            }
        }

        // active resizing
        if (IsResizing)
        {
            bool leftDown = Input.Provider.IsDown(lmb);
            if (!leftDown)
            {
                // finish resizing
                IsResizing = false;
                CurrentResizeEdge = ResizeEdge.None;
                return;
            }

            Input.IsRequestingMouseFocus = true;

            float deltaX = mouse.X - ResizeStartMouse.X;
            float deltaY = mouse.Y - ResizeStartMouse.Y;

            float newX = ResizeStartRect.X;
            float newY = ResizeStartRect.Y;
            float newW = ResizeStartRect.Width;
            float newH = ResizeStartRect.Height;

            switch (CurrentResizeEdge)
            {
                case ResizeEdge.Left:
                    newX = ResizeStartRect.X + deltaX;
                    newW = ResizeStartRect.Width - deltaX;
                    break;
                case ResizeEdge.Right:
                    newW = ResizeStartRect.Width + deltaX;
                    break;
                case ResizeEdge.Top:
                    newY = ResizeStartRect.Y + deltaY;
                    newH = ResizeStartRect.Height - deltaY;
                    break;
                case ResizeEdge.Bottom:
                    newH = ResizeStartRect.Height + deltaY;
                    break;
                case ResizeEdge.TopLeft:
                    newX = ResizeStartRect.X + deltaX;
                    newW = ResizeStartRect.Width - deltaX;
                    newY = ResizeStartRect.Y + deltaY;
                    newH = ResizeStartRect.Height - deltaY;
                    break;
                case ResizeEdge.TopRight:
                    newW = ResizeStartRect.Width + deltaX;
                    newY = ResizeStartRect.Y + deltaY;
                    newH = ResizeStartRect.Height - deltaY;
                    break;
                case ResizeEdge.BottomLeft:
                    newX = ResizeStartRect.X + deltaX;
                    newW = ResizeStartRect.Width - deltaX;
                    newH = ResizeStartRect.Height + deltaY;
                    break;
                case ResizeEdge.BottomRight:
                    newW = ResizeStartRect.Width + deltaX;
                    newH = ResizeStartRect.Height + deltaY;
                    break;
            }

            // enforce minimums
            if (newW < MIN_WIDTH)
            {
                // if dragging left, keep right edge fixed
                if (CurrentResizeEdge == ResizeEdge.Left || CurrentResizeEdge == ResizeEdge.TopLeft || CurrentResizeEdge == ResizeEdge.BottomLeft)
                    newX = ResizeStartRect.X + (ResizeStartRect.Width - MIN_WIDTH);
                newW = MIN_WIDTH;
            }

            if (newH < MIN_HEIGHT)
            {
                if (CurrentResizeEdge == ResizeEdge.Top || CurrentResizeEdge == ResizeEdge.TopLeft || CurrentResizeEdge == ResizeEdge.TopRight)
                    newY = ResizeStartRect.Y + (ResizeStartRect.Height - MIN_HEIGHT);
                newH = MIN_HEIGHT;
            }

            // apply new rect
            CurrentPosition = new Rectangle(newX, newY, newW, newH);
        }
    }

    protected virtual void AfterResize() { }

    protected internal virtual void HandleMovement()
    {
        if (!NoAutoMove)
        {
            AnimationElapsed += Time.deltaTime;
            float tNormalized = Math.Clamp(
                AnimationElapsed / (IsClosing ?
                                            Style.AnimateOutDuration
                                            : Style.AnimateInDuration), 0f, 1f);

            Vector2 newPos = Vector2.Lerp(CurrentPosition.Position, TargetPosition, Style.MoveAndScaleCurve.Evaluate(tNormalized));
            SetPosition(newPos);
            ComputeSize(tNormalized);
        }
    }

    public void SetPosition(Vector2 position) => curPos.Position = position;

    public void SetSize(Vector2 size)
    {
        size.X = Math.Clamp(size.X, 0, float.MaxValue);
        size.Y = Math.Clamp(size.Y, 0, float.MaxValue);
        curPos.Size = size;
    }

    private void ComputeSize(float tNormalized)
    {
        if (!Style.AutoScale || CurrentSize == TargetSize)
            return;

        var center = new Vector2(
            CurrentPosition.X + CurrentPosition.Width / 2f,
            CurrentPosition.Y + CurrentPosition.Height / 2f
        );
        
        CurrentSize = Vector2.Lerp(
            CurrentSize,
            TargetSize,
            Curves.EaseOutBackFar.Evaluate(tNormalized)
        );

        // recompute rect with locked center using absolute sizes
        float newWidth = CurrentSize.X;
        float newHeight = CurrentSize.Y;

        CurrentPosition = new Rectangle(
            center.X - newWidth / 2f,
            center.Y - newHeight / 2f,
            newWidth,
            newHeight
        );
    }

    protected virtual void  HandleContainerDragging()
    {
        if (!Style.AllowUserResizing)
        {
            IsBeingDragged = false;
            IsDragTarget = false;
            return;
        }

        if (!IsBeingDragged)
        {
            if (!IsDragTarget)
            {
                return;
            }

            if (Input.IsPressed(MouseButton.Left))
            {
                // dragging initiated. this block is called once at the start of a drag
                OnContainerDragStart();
                PauseAutoDismissTimer = true;
                IsBeingDragged = true;
            }
            else
                return; // if not clicked, skip dragging
        }

        if (!Input.Provider.IsDown(new InputBinding(InputDeviceType.Mouse, (int)MouseButton.Left)))
        {
            IsBeingDragged = false;
            PauseAutoDismissTimer = false;
            OnContainerDragEnd();
            return;
        }

        IsDragTarget = true;

        //Console.WriteLine(PauseAutoDismissTimer);
        if(prevPauseDragMovement != PauseAutoDismissTimer && !PauseAutoDismissTimer && !Style.PauseAutoDismissTimer)
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
        float visibleStartY = CurrentPosition.Y + UIConstants.CONTENT_PADDING;
        if (Style.AllowUserResizing)
            visibleStartY += Style.TitleBarHeight;

        float dragHeightLocal = Style.AllowUserResizing ? Style.TitleBarHeight : 0f;
        float visibleHeight = CurrentPosition.Height - UIConstants.CONTENT_PADDING * 2 - dragHeightLocal;
        float availableContentWidthCandidate = CurrentPosition.Width - UIConstants.CONTENT_PADDING * 2;

        float totalContentHeight = 0f;
        foreach (var content in Contents)
        {
            totalContentHeight += content.GetHeight(availableContentWidthCandidate);
            totalContentHeight += UIConstants.CONTENT_PADDING;
        }

        if (totalContentHeight > visibleHeight && Style.ShowVerticalScrollBar)
        {
            IsScrollbarVisible = true;
            float reserved = ScrollbarCurrentWidth + UIConstants.CONTENT_PADDING;
            float availableContentWidth = Math.Max(0f, availableContentWidthCandidate - reserved);

            totalContentHeight = 0f;
            foreach (var content in Contents)
            {
                totalContentHeight += content.GetHeight(availableContentWidth);
                totalContentHeight += UIConstants.CONTENT_PADDING;
            }

            LastTotalContentHeight = totalContentHeight;
        }
        else
        {
            IsScrollbarVisible = false;
            LastTotalContentHeight = totalContentHeight;
        }

        var mouse = Input.MousePosition;

        float trackCenterX = CurrentPosition.X + CurrentPosition.Width - UIConstants.CONTENT_PADDING - (ScrollbarCurrentWidth / 2f);
        float trackTopY = visibleStartY;
        float trackBottomY = visibleStartY + visibleHeight;

        bool nearX = Math.Abs(mouse.X - trackCenterX) <= (ScrollbarCurrentWidth / 2f);
        bool withinY = mouse.Y >= trackTopY && mouse.Y <= trackBottomY;
        bool isHoveringScrollbar = IsScrollDragging || (IsScrollbarVisible && nearX && withinY);
        if (isHoveringScrollbar)
        {
            scrollbarHoverTimer += Time.deltaTime;
            if (scrollbarHoverTimer >= SCROLLBAR_HOVER_DELAY)
            {
                scrollbarHoverTimer = SCROLLBAR_HOVER_DELAY;
                ScrollbarHoverTarget = true;
            }
        }
        else
        {
            scrollbarHoverTimer -= Time.deltaTime;
            if (scrollbarHoverTimer <= 0f)
            {
                scrollbarHoverTimer = 0f;
                ScrollbarHoverTarget = false;
            }
        }

        float target = ScrollbarHoverTarget ? 1f : 0f;
        if (ScrollbarAnimProgress != target)
        {
            float delta = Time.deltaTime / Math.Max(0.0001f, SCROLLBAR_ANIM_DURATION);
            if (target > ScrollbarAnimProgress)
                ScrollbarAnimProgress = Math.Min(1f, ScrollbarAnimProgress + delta);
            else
                ScrollbarAnimProgress = Math.Max(0f, ScrollbarAnimProgress - delta);
        }

        float eased = Curves.Linear.Evaluate(ScrollbarAnimProgress);
        ScrollbarCurrentWidth = Lerp(SCROLLBAR_COLLAPSED_WIDTH, SCROLLBAR_EXPANDED_WIDTH, eased);

        if (IsHovered())
        {
            float wheel = Input.ScrollDelta;
            if (Math.Abs(wheel) > 0.001f)
            {
                ContentScrollY -= wheel * SCROLL_WHEEL_SPEED;
                ClampContentScroll();
            }
        }

        float contentOffsetY = visibleStartY - ContentScrollY;

        float contentX = CurrentPosition.X + UIConstants.CONTENT_PADDING;
        float contentWidth = availableContentWidthCandidate;
        if (IsScrollbarVisible)
            contentWidth = Math.Max(0f, availableContentWidthCandidate - (ScrollbarCurrentWidth + UIConstants.CONTENT_PADDING));

        foreach (var content in Contents)
        {
            float contentHeight = content.GetHeight(contentWidth);
            Rectangle contentBounds = new Rectangle(
                contentX,
                contentOffsetY,
                contentWidth,
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
                foreach (var button in Enum.GetValues<MouseButton>())
                    if (Input.IsPressed(button))
                        content.OnClickedOutsideOfContent(button);

                if (content.IsHovered)
                    content.OnHoverEnd();
                content.IsHovered = false;
            }

            content.Update();
            contentOffsetY += contentHeight + UIConstants.CONTENT_PADDING;
        }

        // ensure scroll is valid after layout changes
        ClampContentScroll();
    }

    protected static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    protected internal virtual void Draw()
    {
        float hoverOffsetX = 0, hoverOffsetY = 0;
        float shadowOffsetX = 0, shadowOffsetY = 0;
        HandleRaiseAnimation(ref hoverOffsetX, ref hoverOffsetY, ref shadowOffsetX, ref shadowOffsetY);

        var backgroundBounds = new Rectangle(
            CurrentPosition.X + hoverOffsetX,
            CurrentPosition.Y + hoverOffsetY,
            CurrentPosition.Width,
            CurrentPosition.Height);

        Rectangle dragBounds = new Rectangle(
                backgroundBounds.X + Style.BorderSize,
                backgroundBounds.Y + Style.BorderSize,
                backgroundBounds.Width - Style.BorderSize * 2,
                Style.TitleBarHeight - Style.BorderSize);

        var shadowRect = new Rectangle(
            backgroundBounds.X - Style.ShadowSizeLeft + shadowOffsetX * 2,
            backgroundBounds.Y - Style.ShadowSizeTop + shadowOffsetY * 2,
            backgroundBounds.Width + Style.ShadowSizeLeft + Style.ShadowSizeRight,
            backgroundBounds.Height + Style.ShadowSizeTop + Style.ShadowSizeBottom
        );
        ray.DrawRectangleRec(shadowRect, Style.Shadow);

        ray.DrawRectangleRec(backgroundBounds, Style.Background);
        ray.DrawRectangleLinesEx(backgroundBounds, 2, Style.Border);
        HandleTitleBar(backgroundBounds, dragBounds);

        // IMPORTANT: bottom-most content area must be background - UIConstants.CONTENT_PADDING regardless of titlebar
        float contentY = backgroundBounds.Y + UIConstants.CONTENT_PADDING + (Style.AllowUserResizing ? Style.TitleBarHeight : 0f);
        float contentHeight = backgroundBounds.Y + backgroundBounds.Height - UIConstants.CONTENT_PADDING - contentY;
        float contentWidth = backgroundBounds.Width - UIConstants.CONTENT_PADDING * 2;

        if (IsScrollbarVisible)
            contentWidth = Math.Max(0f, contentWidth - (ScrollbarCurrentWidth + UIConstants.CONTENT_PADDING));

        Rectangle bounds = new Rectangle(
            backgroundBounds.X + UIConstants.CONTENT_PADDING,
            contentY,
            contentWidth,
            contentHeight
        );

        LastContentRenderBounds = bounds;
        LastBorderBounds = backgroundBounds;
        DrawContent(bounds);
        if (EnableDebugDraw && Style.AllowUserResizing)
            DrawResizeDebugRects(LastBorderBounds);

        if (Style.TimeUntilAutoDismiss > 0)
            DrawCloseTimerBar(backgroundBounds);
    }

    protected void DrawResizeDebugRects(Rectangle r)
    {
        float m = RESIZE_MARGIN;
        var semi = new Color(255, 0, 0, RESIZE_DEBUG_ALPHA);

        // left edge (extends slightly outward)
        ray.DrawRectangleRec(new Rectangle(r.X - m, r.Y - m, m, r.Height + m * 2f), semi);

        // right edge
        ray.DrawRectangleRec(new Rectangle(r.X + r.Width, r.Y - m, m, r.Height + m * 2f), semi);

        // top edge
        ray.DrawRectangleRec(new Rectangle(r.X - m, r.Y - m, r.Width + m * 2f, m), semi);

        // bottom edge
        ray.DrawRectangleRec(new Rectangle(r.X - m, r.Y + r.Height, r.Width + m * 2f, m), semi);

        // corner indicators (slightly larger)
        float corner = m * 1.5f;
        ray.DrawRectangleRec(new Rectangle(r.X - corner, r.Y - corner, corner, corner), semi); // top-left
        ray.DrawRectangleRec(new Rectangle(r.X + r.Width, r.Y - corner, corner, corner), semi); // top-right
        ray.DrawRectangleRec(new Rectangle(r.X - corner, r.Y + r.Height, corner, corner), semi); // bottom-left
        ray.DrawRectangleRec(new Rectangle(r.X + r.Width, r.Y + r.Height, corner, corner), semi); // bottom-right
    }

    protected void DrawCloseTimerBar(Rectangle r)
    {
        float progressRatio = Math.Clamp(TimeShown / Style.TimeUntilAutoDismiss, 0f, 1f);
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

    protected virtual bool DrawCustomTitleBar(Rectangle titlebarBounds) { return true; }

    protected void HandleTitleBar(Rectangle backgroundBounds, Rectangle dragBounds)
    {
        if (Style.AllowUserResizing)
        {
            bool shouldDrag = DrawCustomTitleBar(dragBounds);

            bool hoveringDragTarget = false;
            if (Style.AllowUserResizing && shouldDrag)
                hoveringDragTarget = ray.CheckCollisionPointRec(Input.MousePosition, dragBounds);

            if (hoveringDragTarget != IsDragTarget)
            {
                IsDragTarget = hoveringDragTarget;
            }
        }
    }

    protected void HandleRaiseAnimation(ref float hoverOffsetX, ref float hoverOffsetY, ref float shadowOffsetX, ref float shadowOffsetY)
    {
        if (Style.RaiseOnHover)
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
        // Begin scissor so nothing draws outside the content area
        ScissorStack.Push(contentArea);

        float offsetY = contentArea.Y - ContentScrollY;
        foreach (var content in Contents)
        {
            float contentHeight = content.GetSize(contentArea).Y;
            Rectangle bounds = new Rectangle(contentArea.X, offsetY, contentArea.Width, contentHeight);

            if (EnableDebugDraw)
                ray.DrawRectangleLinesEx(bounds, 1, Color.Beige);

            content.InternalDraw(bounds);
            offsetY += contentHeight + UIConstants.CONTENT_PADDING;
        }

        ScissorStack.Pop();

        // Draw scrollbars on top of the container but not over the titlebar
        DrawVerticalScrollbar(contentArea, LastBorderBounds);
    }

    private void ClampContentScroll()
    {
        float dragHeightLocal = Style.AllowUserResizing ? Style.TitleBarHeight : 0f;
        float visibleHeight = CurrentPosition.Height - UIConstants.CONTENT_PADDING * 2 - dragHeightLocal;
        float maxScroll = Math.Max(0f, LastTotalContentHeight - visibleHeight);
        if (ContentScrollY < 0f) ContentScrollY = 0f;
        if (ContentScrollY > maxScroll) ContentScrollY = maxScroll;
    }

    protected virtual void DrawVerticalScrollbar(Rectangle contentArea, Rectangle backgroundBounds)
    {
        if (LastTotalContentHeight <= contentArea.Height + 1 || !Style.ShowVerticalScrollBar)
            return;

        // track area aligned with contentArea (so it doesn't overlap titlebar)
        float trackX = backgroundBounds.X + backgroundBounds.Width - ScrollbarCurrentWidth - UIConstants.CONTENT_PADDING;
        float trackY = contentArea.Y;
        float trackHeight = contentArea.Height;

        var trackRect = new Rectangle(trackX, trackY, ScrollbarCurrentWidth, trackHeight);

        // thumb size and position
        float visibleHeight = contentArea.Height;
        float proportionVisible = visibleHeight / LastTotalContentHeight;
        float thumbHeight = Math.Max(SCROLL_MIN_THUMB, trackHeight * proportionVisible);

        float maxScroll = Math.Max(0f, LastTotalContentHeight - visibleHeight);
        float scrollRatio = maxScroll > 0f ? (ContentScrollY / maxScroll) : 0f;
        float thumbY = trackY + scrollRatio * (trackHeight - thumbHeight);

        var thumbRect = new Rectangle(trackX, thumbY, ScrollbarCurrentWidth, thumbHeight);

        // colors (use style colors where available)
        var trackColor = Style.ScrollbarTrack;
        var thumbColor = Style.ScrollbarThumb;

        // draw track and thumb (slightly inset so the expanded thumb looks nicer)
        var inset = 1f;
        ray.DrawRectangleRec(new Rectangle(trackRect.X + inset, trackRect.Y + inset, trackRect.Width - inset * 2f, trackRect.Height - inset * 2f), trackColor);
        ray.DrawRectangleRec(new Rectangle(thumbRect.X + inset, thumbRect.Y + inset, thumbRect.Width - inset * 2f, thumbRect.Height - inset * 2f), thumbColor);
        ray.DrawRectangleLinesEx(new Rectangle(trackRect.X, trackRect.Y, trackRect.Width, trackRect.Height), 1, Style.Border);

        // --- Interactivity: clicking & dragging the thumb ---
        var mouse = Input.MousePosition;

        // begin drag
        if (Input.IsPressed(MouseButton.Left))
        {
            if (ray.CheckCollisionPointRec(mouse, thumbRect))
            {
                IsScrollDragging = true;
                ScrollDragOffset = mouse.Y - thumbRect.Y;
            }
            else if (ray.CheckCollisionPointRec(mouse, trackRect))
            {
                // jump-to-click on track (center thumb on click)
                float clickedNormalized = (mouse.Y - trackY) / (trackHeight - thumbHeight);
                clickedNormalized = Math.Clamp(clickedNormalized, 0f, 1f);
                ContentScrollY = clickedNormalized * maxScroll;
                ClampContentScroll();
            }
        }

        // dragging
        if (IsScrollDragging)
        {
            bool leftDown = Input.Provider.IsDown(new InputBinding(InputDeviceType.Mouse, (int)MouseButton.Left));
            if (!leftDown)
            {
                IsScrollDragging = false;
            }
            else
            {
                float dragY = mouse.Y - ScrollDragOffset;
                float normalized = (dragY - trackY) / (trackHeight - thumbHeight);
                normalized = Math.Clamp(normalized, 0f, 1f);
                ContentScrollY = normalized * maxScroll;
                ClampContentScroll();
            }
        }
    }

    public virtual bool IsHovered() => Input.IsMouseHovering(CurrentPosition);

    public virtual void Close()
    {
        if (!IsClosing)
        {
            foreach (var c in Contents)
                c.OnOwnerClosing();

            IsClosing = true;
            if(this is Toast)
            {
                isHoverTarget = true;
                AnimationElapsed = 0f;
            }
        }
    }

    protected virtual void HandleAutoClose()
    {
        if (!PauseAutoDismissTimer && !Style.PauseAutoDismissTimer && Style.TimeUntilAutoDismiss > 0 && !IsHovered())
        {
            TimeShown += Time.deltaTime;
            if (TimeShown >= Style.TimeUntilAutoDismiss)
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

    public UIContainer AddButton(RichText text, VoidInvocation<UIContainer, UIButton>? onClick = null)
    {
        AddContent(new UIButton(text, onClick));
        return this;
    }

    /// <summary>
    /// Adds a button to the toast
    /// </summary>
    /// <param name="text"></param>
    /// <param name="onClick">Should return true when the toast should close, false if not</param>
    /// <returns></returns>
    public UIContainer AddButton(string text, VoidInvocation<UIContainer, UIButton>? onClick) => AddButton(RichText.Parse(text, Color.White), onClick);

    /// <summary>
    /// Adds a progress bar to the toast
    /// </summary>
    /// <param name="initialProgress">The progress in a 0-1 range. set to -1 to have it do a infinite working animation</param>
    /// <param name="ProgressProvider">The function that provides further values</param>
    /// <param name="closesToastWhenComplete">When true, and the progress becomes 1, it requests the toast to close.</param>
    /// <returns></returns>
    public UIContainer AddProgressBar(float initialProgress, Func<float, float>? ProgressProvider = null, string infiniteSpinText = "Working...")
    {
        return AddContent(new UIProgress(initialProgress, ProgressProvider, infiniteSpinText));
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
        => AddContent(new UIText(text, preset));
    public UIContainer AddText(RichText text, UIFontSizePreset preset = UIFontSizePreset.Text)
        => AddContent(new UIText(text, preset));

    public UIContainer AddText(string text, UIFontSizePreset preset = UIFontSizePreset.Text)
        => AddText(RichText.Parse(text, Color.White), preset);
}
