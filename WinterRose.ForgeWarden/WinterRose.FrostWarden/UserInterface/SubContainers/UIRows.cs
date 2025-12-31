using Raylib_cs;
using WinterRose.ForgeWarden.Lighting;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.Utility;

namespace WinterRose.ForgeWarden.UserInterface;


public class UIRows : UIContent
{
    // configuration
    public const int DEFAULT_ROW_COUNT = 1;
    public const float DEFAULT_ROW_HEIGHT = 80f; // fixed row height
    public int RowCount { get; set; } = DEFAULT_ROW_COUNT;
    /// <summary>
    /// If > 0 each row will use this fixed height (already fixed by design). Kept for parity.
    /// </summary>
    public float FixedRowHeight { get; set; } = DEFAULT_ROW_HEIGHT;
    public float RowSpacing { get; set; } = 6f;
    public float RowPadding { get; set; } = UIConstants.CONTENT_PADDING;

    // scrollbar animation constants (container-level vertical scrollbar and per-row horizontal scrollbars)
    private const float SCROLLBAR_HOVER_DELAY = 0.25f;
    private const float SCROLLBAR_COLLAPSED_THICKNESS = 6f;
    private const float SCROLLBAR_EXPANDED_THICKNESS = 12f;
    private const float SCROLL_WHEEL_SPEED = 24f;
    private const float SCROLLBAR_ANIM_DURATION = 0.12f;

    // rows content storage
    public List<List<UIContent>> RowsContents { get; } = new();

    // per-row runtime state (horizontal scrolling inside a row)
    private class RowState
    {
        public float ContentScrollX = 0f;
        public bool IsScrollbarVisible = false;
        public float ScrollbarCurrentThickness = SCROLLBAR_COLLAPSED_THICKNESS;
        public float ScrollbarAnimProgress = 0f;
        public float ScrollbarHoverTimer = 0f;
        public bool ScrollbarHoverTarget = false;
        public bool IsScrollDragging = false;
        public float LastTotalContentWidth = 0f;

        public bool AutoScrollEnabled = false;    // toggle per row
        public float AutoScrollDirection = 1f;

        public float RowHeightOverride = 0f;

        public bool AutoSizeToContent = false;
        public float CachedAutoHeight = 0f;
    }

    private readonly List<RowState> rowStates = new();

    // container-level state (vertical scrolling of the whole rows collection)
    private class ContainerState
    {
        public float ContentScrollY = 0f;
        public bool IsScrollbarVisible = false;
        public float ScrollbarCurrentWidth = SCROLLBAR_COLLAPSED_THICKNESS;
        public float ScrollbarAnimProgress = 0f;
        public float ScrollbarHoverTimer = 0f;
        public bool ScrollbarHoverTarget = false;
        public bool IsScrollDragging = false;
        public float LastTotalRowsHeight = 0f;

        public float ScrollProgress { get; internal set; }
    }

    private readonly ContainerState containerState = new();

    public UIRows()
    {
        EnsureRows(RowCount);
    }

    public void SetRowHeight(int rowIndex, float height)
    {
        EnsureRows(rowIndex + 1);
        rowStates[rowIndex].RowHeightOverride = height;
    }

    public void SetRowContentScroll(int rowIndex, bool doScroll)
    {
        EnsureRows(rowIndex + 1);
        rowStates[rowIndex].AutoScrollEnabled = doScroll;
    }

    public void ClearRowHeight(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= rowStates.Count) return;
        rowStates[rowIndex].RowHeightOverride = 0f;
    }

    public void ClearRowContentScroll(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= rowStates.Count) return;
        rowStates[rowIndex].AutoScrollEnabled = false;
    }

    public void SetRowAutoSize(int rowIndex, bool enabled)
    {
        EnsureRows(rowIndex + 1);
        rowStates[rowIndex].AutoSizeToContent = enabled;
    }

    public void ClearRowAutoSize(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= rowStates.Count) return;
        rowStates[rowIndex].AutoSizeToContent = false;
    }

    protected internal override void Setup()
    {
        base.Setup();
        EnsureRows(RowCount);

        foreach(var row in RowsContents)
        {
            foreach(UIContent c in row)
            {
                c.Owner = Owner;
                c.Setup();
            }
        }
    }

    public void MoveRows(int byRows)
    {
        if (byRows == 0) return;

        if (byRows > 0)
        {
            // prepend rows at the start
            for (int i = 0; i < byRows; i++)
            {
                RowsContents.Insert(0, new List<UIContent>());
                rowStates.Insert(0, new RowState());
            }
        }
        else
        {
            // append rows at the end
            for (int i = 0; i < -byRows; i++)
            {
                RowsContents.Add(new List<UIContent>());
                rowStates.Add(new RowState());
            }
        }

        // keep RowCount in sync
        RowCount = RowsContents.Count;

        // ensure minimum rows if needed
        if (RowCount == 0)
            EnsureRows(DEFAULT_ROW_COUNT);
    }

    private void EnsureRows(int count)
    {
        if (count <= 0) count = DEFAULT_ROW_COUNT;

        while (RowsContents.Count < count)
            RowsContents.Add(new List<UIContent>());

        while (rowStates.Count < count)
            rowStates.Add(new RowState());

        // keep lists in sync (if RowsContents grows first, ensure rowStates exists; if rows shrink, trim states)
        while (RowsContents.Count > rowStates.Count)
            rowStates.Add(new RowState());

        while (rowStates.Count > RowsContents.Count)
            rowStates.RemoveAt(rowStates.Count - 1);

        RowCount = RowsContents.Count;
        if (RowCount == 0)
            EnsureRows(DEFAULT_ROW_COUNT);
    }

    private float ResolveRowHeight(int rowIndex)
    {
        var rstate = rowStates[rowIndex];

        float minimumHeight = rstate.RowHeightOverride > 0f
            ? rstate.RowHeightOverride
            : FixedRowHeight;

        if (!rstate.AutoSizeToContent)
            return minimumHeight;

        if (rstate.CachedAutoHeight <= 0f)
            return minimumHeight;

        return Math.Max(minimumHeight, rstate.CachedAutoHeight);
    }


    public void AddToRow(int rowIndex, UIContent content)
    {
        if (content == null) return;
        EnsureRows(rowIndex + 1);
        int idx = Math.Clamp(rowIndex, 0, RowCount - 1);
        RowsContents[idx].Add(content);
        content.Owner = Owner;
        content.Setup();
    }

    public void ClearRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= RowsContents.Count) return;
        foreach (var c in RowsContents[rowIndex])
            c.OnOwnerClosing();
        RowsContents[rowIndex].Clear();
    }

    public void RemoveRow(int index)
    {
        if (index < 0 || index >= RowsContents.Count)
            return;

        foreach (var c in RowsContents[index])
            c.OnOwnerClosing();

        RowsContents.RemoveAt(index);
        rowStates.RemoveAt(index);

        RowCount = RowsContents.Count;
        if (RowCount == 0)
            EnsureRows(DEFAULT_ROW_COUNT);
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        float width = availableArea.Width;

        // height is fixed per-row, so total height is rows*height + spacing
        float totalRowsHeight = 0f;

        for (int i = 0; i < RowCount; i++)
            totalRowsHeight += ResolveRowHeight(i);

        totalRowsHeight += Math.Max(0, RowCount - 1) * RowSpacing;

        // if there are no children, give at least one row default height
        if (RowCount == 0)
            totalRowsHeight = DEFAULT_ROW_HEIGHT;

        return new Vector2(width, totalRowsHeight);
    }

    protected internal override float GetHeight(float maxWidth)
    {
        return GetSize(new Rectangle(0, 0, (int)maxWidth, int.MaxValue)).Y;
    }

    protected internal override void Update()
    {
        // Update children logic (non-visual updates)
        foreach (var row in RowsContents)
        {
            foreach (var child in row)
                child.Update();
        }
    }

    protected internal override void OnOwnerClosing()
    {
        foreach (var row in RowsContents)
            foreach (var child in row)
                child.OnOwnerClosing();
    }

    protected internal override void OnHoverEnd()
    {
        foreach (var row in RowsContents)
            foreach (var child in row)
                child.OnHoverEnd();
    }

    protected internal override void OnClickedOutsideOfContent(MouseButton button)
    {
        foreach (var row in RowsContents)
            foreach (var child in row)
                child.OnClickedOutsideOfContent(button);
    }

    protected override void Draw(Rectangle bounds)
    {
        if (RowCount <= 0) return;
        EnsureRows(RowCount);

        float totalRowsHeight = 0f;

        for (int i = 0; i < RowCount; i++)
            totalRowsHeight += ResolveRowHeight(i);


        totalRowsHeight += RowSpacing * (RowCount - 1);


        // visible area inside this container
        float visibleStartY = bounds.Y + RowPadding;
        float visibleHeight = bounds.Height - RowPadding * 2f;
        float availableRowWidthCandidate = Math.Max(0f, bounds.Width - RowPadding * 2f);

        // compute total rows height (same as above but kept for state)
        containerState.LastTotalRowsHeight = totalRowsHeight;

        // determine container-level vertical scrollbar
        if (totalRowsHeight > visibleHeight)
        {
            containerState.IsScrollbarVisible = true;
            float reserved = containerState.ScrollbarCurrentWidth + RowPadding;
            float availableRowWidth = Math.Max(0f, availableRowWidthCandidate - reserved);

            // nothing else to recalc horizontally for rows; keep LastTotalRowsHeight
        }
        else
        {
            containerState.IsScrollbarVisible = false;
        }

        // container scrollbar hover/animation
        var mouse = Input.MousePosition;
        float trackCenterX = bounds.X + bounds.Width - RowPadding - (containerState.ScrollbarCurrentWidth / 2f);
        float trackTopY = visibleStartY;
        float trackBottomY = visibleStartY + visibleHeight;

        bool nearX = Math.Abs(mouse.X - trackCenterX) <= (containerState.ScrollbarCurrentWidth / 2f);
        bool withinY = mouse.Y >= trackTopY && mouse.Y <= trackBottomY;
        bool isHoveringContainerScrollbar = containerState.IsScrollDragging || (containerState.IsScrollbarVisible && nearX && withinY);
        if (isHoveringContainerScrollbar)
        {
            containerState.ScrollbarHoverTimer += Time.deltaTime;
            if (containerState.ScrollbarHoverTimer >= SCROLLBAR_HOVER_DELAY)
            {
                containerState.ScrollbarHoverTimer = SCROLLBAR_HOVER_DELAY;
                containerState.ScrollbarHoverTarget = true;
            }
        }
        else
        {
            containerState.ScrollbarHoverTimer -= Time.deltaTime;
            if (containerState.ScrollbarHoverTimer <= 0f)
            {
                containerState.ScrollbarHoverTimer = 0f;
                containerState.ScrollbarHoverTarget = false;
            }
        }

        float containerTarget = containerState.ScrollbarHoverTarget ? 1f : 0f;
        if (containerState.ScrollbarAnimProgress != containerTarget)
        {
            float delta = Time.deltaTime / Math.Max(0.0001f, SCROLLBAR_ANIM_DURATION);
            if (containerTarget > containerState.ScrollbarAnimProgress)
                containerState.ScrollbarAnimProgress = Math.Min(1f, containerState.ScrollbarAnimProgress + delta);
            else
                containerState.ScrollbarAnimProgress = Math.Max(0f, containerState.ScrollbarAnimProgress - delta);
        }

        float containerEased = Curves.Linear.Evaluate(containerState.ScrollbarAnimProgress);
        containerState.ScrollbarCurrentWidth = Lerp(SCROLLBAR_COLLAPSED_THICKNESS, SCROLLBAR_EXPANDED_THICKNESS, containerEased);

        // container-level vertical wheel handling when hovering container
        bool isContainerHovered = IsContentHovered(bounds);
        if (isContainerHovered)
        {
            float wheel = Raylib_cs.Raylib.GetMouseWheelMove();
            if (Math.Abs(wheel) > 0.001f)
            {
                containerState.ContentScrollY -= wheel * SCROLL_WHEEL_SPEED;
                containerState.ContentScrollY = Math.Clamp(containerState.ContentScrollY, 0f, Math.Max(0f, containerState.LastTotalRowsHeight - visibleHeight));
            }
        }

        // start drawing rows; apply container-level scroll
        float rowStartY = bounds.Y + RowPadding - containerState.ContentScrollY;
        float currentRowY = rowStartY;
        for (int ri = 0; ri < RowCount; ri++)
        {
            var rstate2 = rowStates[ri];

            // --- AUTO-SIZE MEASUREMENT (INSERT HERE) ---
            if (rstate2.AutoSizeToContent)
            {
                float maxChildHeight = 0f;
                var rowList2 = RowsContents[ri];

                for (int c = 0; c < rowList2.Count; c++)
                {
                    float h = rowList2[c].GetHeight(bounds.Width - RowPadding * 2f);
                    maxChildHeight = Math.Max(maxChildHeight, h);
                }

                rstate2.CachedAutoHeight = maxChildHeight + RowPadding * 2f;
            }
            // -----------------------------------------

            float rowHeight = ResolveRowHeight(ri);

            Rectangle rowRect = new Rectangle(
                (int)bounds.X,
                (int)currentRowY,
                (int)bounds.Width,
                (int)rowHeight
            );

            currentRowY += rowHeight + RowSpacing;

            // push scissor for this row
            ScissorStack.Push(rowRect);

            // compute visible area inside row
            float visibleRowStartX = rowRect.X + RowPadding;
            float visibleRowWidth = rowRect.Width - RowPadding * 2f;

            var rowList = (ri < RowsContents.Count) ? RowsContents[ri] : new List<UIContent>();

            // measure total content width for this row
            float totalContentWidth = 0f;
            List<float> measuredChildWidths = new List<float>(rowList.Count);
            for (int c = 0; c < rowList.Count; c++)
            {
                var child = rowList[c];
                // measure child width given the row height constraint (subtract padding)
                var childSize = child.GetSize(new Rectangle(0, 0, int.MaxValue, (int)Math.Max(0f, rowHeight - RowPadding * 2f)));
                float childWidth = Math.Max(0f, childSize.X);
                measuredChildWidths.Add(childWidth);
                totalContentWidth += childWidth;
                if (c < rowList.Count - 1)
                    totalContentWidth += RowPadding; // spacing between items inside row
            }

            var rstate = rowStates[ri];

            // determine per-row horizontal scrollbar visibility and recalc if visible
            if (totalContentWidth > visibleRowWidth)
            {
                rstate.IsScrollbarVisible = true;
                float reserved = rstate.ScrollbarCurrentThickness + RowPadding;
                float availableRowWidth = Math.Max(0f, visibleRowWidth - reserved);

                // recalc with availableRowWidth if needed (we only measured preferred widths, assume same)
                rstate.LastTotalContentWidth = totalContentWidth;
            }
            else
            {
                rstate.IsScrollbarVisible = false;
                rstate.LastTotalContentWidth = totalContentWidth;
                rstate.ContentScrollX = 0f;
            }

            // per-row scrollbar hover/animation (horizontal)
            float trackCenterY = rowRect.Y + rowRect.Height - RowPadding - (rstate.ScrollbarCurrentThickness / 2f);
            float trackLeftX = visibleRowStartX;
            float trackRightX = visibleRowStartX + visibleRowWidth;

            bool nearY = Math.Abs(mouse.Y - trackCenterY) <= (rstate.ScrollbarCurrentThickness / 2f);
            bool withinX = mouse.X >= trackLeftX && mouse.X <= trackRightX;
            bool isHoveringRowScrollbar = rstate.IsScrollDragging || (rstate.IsScrollbarVisible && nearY && withinX);
            if (isHoveringRowScrollbar)
            {
                rstate.ScrollbarHoverTimer += Time.deltaTime;
                if (rstate.ScrollbarHoverTimer >= SCROLLBAR_HOVER_DELAY)
                {
                    rstate.ScrollbarHoverTimer = SCROLLBAR_HOVER_DELAY;
                    rstate.ScrollbarHoverTarget = true;
                }
            }
            else
            {
                rstate.ScrollbarHoverTimer -= Time.deltaTime;
                if (rstate.ScrollbarHoverTimer <= 0f)
                {
                    rstate.ScrollbarHoverTimer = 0f;
                    rstate.ScrollbarHoverTarget = false;
                }
            }

            float rtarget = rstate.ScrollbarHoverTarget ? 1f : 0f;
            if (rstate.ScrollbarAnimProgress != rtarget)
            {
                float delta = Time.deltaTime / Math.Max(0.0001f, SCROLLBAR_ANIM_DURATION);
                if (rtarget > rstate.ScrollbarAnimProgress)
                    rstate.ScrollbarAnimProgress = Math.Min(1f, rstate.ScrollbarAnimProgress + delta);
                else
                    rstate.ScrollbarAnimProgress = Math.Max(0f, rstate.ScrollbarAnimProgress - delta);
            }

            float reased = Curves.Linear.Evaluate(rstate.ScrollbarAnimProgress);
            rstate.ScrollbarCurrentThickness = Lerp(SCROLLBAR_COLLAPSED_THICKNESS, SCROLLBAR_EXPANDED_THICKNESS, reased);

            // handle wheel for horizontal scroll on a row when hovering it
            bool isRowHovered = IsContentHovered(rowRect);
            if (isRowHovered)
            {
                float wheel = Raylib_cs.Raylib.GetMouseWheelMove();
                if (Math.Abs(wheel) > 0.001f)
                {
                    // horizontal scroll with wheel (common UX: wheel scrolls vertically, but we allow horizontal in rows)
                    rstate.ContentScrollX -= wheel * SCROLL_WHEEL_SPEED;
                    rstate.ContentScrollX = Math.Clamp(rstate.ContentScrollX, 0f, Math.Max(0f, rstate.LastTotalContentWidth - visibleRowWidth));
                }
            }

            // draw children inside the row with scissor applied and respecting horizontal scroll
            float contentOffsetX = visibleRowStartX - rstate.ContentScrollX;
            for (int c = 0; c < rowList.Count; c++)
            {
                var child = rowList[c];
                float childWidth = measuredChildWidths[c];
                float childHeight = Math.Max(0f, rowHeight - RowPadding * 2f);

                Rectangle childBounds = new Rectangle(
                    (int)contentOffsetX,
                    (int)(rowRect.Y + RowPadding),
                    (int)childWidth,
                    (int)childHeight
                );

                if (child.IsContentHovered(childBounds))
                {
                    child.OnHover();
                    child.IsHovered = true;

                    foreach (var button in Enum.GetValues<MouseButton>())
                    {
                        if (Input.IsPressed(button))
                            child.OnContentClicked(button);
                    }
                }
                else
                {
                    foreach (var button in Enum.GetValues<MouseButton>())
                        if (Input.IsPressed(button))
                            child.OnClickedOutsideOfContent(button);

                    if (child.IsHovered)
                        child.OnHoverEnd();
                    child.IsHovered = false;
                }

                child.InternalDraw(childBounds);

                contentOffsetX += childWidth + RowPadding;
            }

            // pop scissor for this row
            ScissorStack.Pop();
        }

        // clamp container scroll after layout changes
        containerState.ContentScrollY = Math.Clamp(containerState.ContentScrollY, 0f, Math.Max(0f, containerState.LastTotalRowsHeight - visibleHeight));

        containerState.ScrollProgress = (containerState.LastTotalRowsHeight <= visibleHeight)
            ? 0f
            : containerState.ContentScrollY / (containerState.LastTotalRowsHeight - visibleHeight);

        for (int ri = 0; ri < rowStates.Count; ri++)
        {
            var rstate = rowStates[ri];

            float visibleRowWidth = bounds.Width - RowPadding * 2f; // or precompute per row
            float maxScrollX = Math.Max(0f, rstate.LastTotalContentWidth - visibleRowWidth);

            // Manual scroll clamp
            rstate.ContentScrollX = Math.Clamp(rstate.ContentScrollX, 0f, maxScrollX);

            // Auto-scroll
            if (rstate.AutoScrollEnabled && maxScrollX > 0f)
            {
                rstate.ContentScrollX += rstate.AutoScrollDirection * Style.AutoScrollSpeed * Time.deltaTime;

                // Reverse direction at edges
                if (rstate.ContentScrollX >= maxScrollX)
                {
                    rstate.ContentScrollX = maxScrollX;
                    rstate.AutoScrollDirection = -1f;
                }
                else if (rstate.ContentScrollX <= 0f)
                {
                    rstate.ContentScrollX = 0f;
                    rstate.AutoScrollDirection = 1f;
                }
            }
        }

    }

    // small helper for lerp
    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}

