using Raylib_cs;
using WinterRose.ForgeWarden.Lighting;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.Utility;

namespace WinterRose.ForgeWarden.UserInterface;


public class UIRows : UIContent, IUIContainer
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
    public bool AllowVerticalScroll { get; set; } = false;

    // scrollbar animation constants (container-level vertical scrollbar and per-row horizontal scrollbars)
    private const float SCROLLBAR_HOVER_DELAY = 0.25f;
    private const float SCROLLBAR_COLLAPSED_THICKNESS = 6f;
    private const float SCROLLBAR_EXPANDED_THICKNESS = 12f;
    private const float SCROLL_WHEEL_SPEED = 24f;
    private const float SCROLLBAR_ANIM_DURATION = 0.12f;

    public List<bool> RowScrollEnabled { get; } = new();

    private bool setupCalled = false;

    // rows content storage
    public List<List<UIContent>> RowsContents { get; } = new();

    public bool IsVisible => Owner.IsVisible;

    public bool IsClosing => Owner.IsClosing;

    public bool IsBeingDragged => Owner.IsBeingDragged;

    public bool PauseDragMovement => Owner.PauseDragMovement;

    public Rectangle CurrentPosition => Owner.CurrentPosition;

    public float Height => GetHeight(CurrentPosition.Width);

    public IReadOnlyList<UIContent> Contents
    {
        get
        {
            List<UIContent> flattened = new List<UIContent>();

            foreach (var row in RowsContents)
                flattened.AddRange(row);

            return flattened;
        }
    }

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

        foreach (var row in RowsContents)
        {
            foreach (UIContent c in row)
            {
                // owner should be the UIRows instance (parity with UIColumns)
                c.Owner = this;
                c.Setup();
            }
        }

        setupCalled = true;
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

        // ensure auxiliary lists (RowScrollEnabled etc) are in sync
        EnsureRows(RowCount);
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

        // ---- ROW SCROLL ENABLE LIST SYNC (parity with ColumnScrollEnabled) ----
        while (RowScrollEnabled.Count < count)
            RowScrollEnabled.Add(true);

        while (RowScrollEnabled.Count > RowsContents.Count)
            RowScrollEnabled.RemoveAt(RowScrollEnabled.Count - 1);
        // ---------------------------------------------------------------------

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

        // owner should be this and only call Setup if this container already ran Setup()
        content.Owner = this;
        if (setupCalled)
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

        // mirror removal for RowScrollEnabled
        if (index < RowScrollEnabled.Count)
            RowScrollEnabled.RemoveAt(index);

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

    protected override void Update()
    {
        // Update children logic (non-visual updates)
        foreach (var row in RowsContents)
        {
            foreach (var child in row)
                child._Update();
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
        if (RowCount <= 0)
            return;

        EnsureRows(RowCount);

        float visibleStartY = bounds.Y + RowPadding;
        float visibleHeight = bounds.Height - RowPadding * 2f;
        float availableRowWidth = Math.Max(0f, bounds.Width - RowPadding * 2f);

        //
        // ------------------------------------------------------------------
        // 1. MEASURE TOTAL ROW HEIGHT
        // ------------------------------------------------------------------
        //

        float totalRowsHeight = 0f;

        for (int i = 0; i < RowCount; i++)
        {
            // Auto-size support
            var rstate = rowStates[i];
            if (rstate.AutoSizeToContent)
            {
                float maxChildHeight = 0f;
                var rowList = RowsContents[i];

                for (int c = 0; c < rowList.Count; c++)
                {
                    float h = rowList[c].GetHeight(availableRowWidth);
                    maxChildHeight = Math.Max(maxChildHeight, h);
                }

                rstate.CachedAutoHeight = maxChildHeight + RowPadding * 2f;
            }

            totalRowsHeight += ResolveRowHeight(i);
        }

        totalRowsHeight += RowSpacing * (RowCount - 1);

        containerState.LastTotalRowsHeight = totalRowsHeight;

        //
        // ------------------------------------------------------------------
        // 2. DETERMINE VERTICAL SCROLLBAR
        // ------------------------------------------------------------------
        //

        containerState.IsScrollbarVisible = totalRowsHeight > visibleHeight;

        float containerReservedWidth = 0f;

        if (containerState.IsScrollbarVisible)
            containerReservedWidth = containerState.ScrollbarCurrentWidth + RowPadding;

        //
        // ------------------------------------------------------------------
        // 3. CONTAINER SCROLLBAR ANIMATION
        // ------------------------------------------------------------------
        //

        var mouse = Input.MousePosition;

        float trackCenterX = bounds.X + bounds.Width - RowPadding - (containerState.ScrollbarCurrentWidth / 2f);
        float trackTopY = visibleStartY;
        float trackBottomY = visibleStartY + visibleHeight;

        bool nearX = Math.Abs(mouse.X - trackCenterX) <= (containerState.ScrollbarCurrentWidth / 2f);
        bool withinY = mouse.Y >= trackTopY && mouse.Y <= trackBottomY;

        bool hoveringScrollbar =
            containerState.IsScrollDragging ||
            (containerState.IsScrollbarVisible && nearX && withinY);

        if (hoveringScrollbar)
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

        float target = containerState.ScrollbarHoverTarget ? 1f : 0f;

        if (containerState.ScrollbarAnimProgress != target)
        {
            float delta = Time.deltaTime / Math.Max(0.0001f, SCROLLBAR_ANIM_DURATION);

            if (target > containerState.ScrollbarAnimProgress)
                containerState.ScrollbarAnimProgress = Math.Min(1f, containerState.ScrollbarAnimProgress + delta);
            else
                containerState.ScrollbarAnimProgress = Math.Max(0f, containerState.ScrollbarAnimProgress - delta);
        }

        float eased = Curves.Linear.Evaluate(containerState.ScrollbarAnimProgress);

        containerState.ScrollbarCurrentWidth =
            Lerp(SCROLLBAR_COLLAPSED_THICKNESS, SCROLLBAR_EXPANDED_THICKNESS, eased);

        //
        // ------------------------------------------------------------------
        // 4. VERTICAL SCROLL WHEEL HANDLING
        // ------------------------------------------------------------------
        //

        float wheel = Input.ScrollDelta;

        if (Math.Abs(wheel) > 0.001f && AllowVerticalScroll)
        {
            bool consumed = false;

            float rowY = visibleStartY - containerState.ContentScrollY;

            for (int ri = 0; ri < RowCount; ri++)
            {
                float rowHeight = ResolveRowHeight(ri);

                Rectangle rowRect = new Rectangle(
                    (int)bounds.X,
                    (int)rowY,
                    (int)bounds.Width,
                    (int)rowHeight
                );

                if (IsContentHovered(rowRect))
                {
                    containerState.ContentScrollY -= wheel * SCROLL_WHEEL_SPEED;

                    float maxScroll =
                        Math.Max(0f, containerState.LastTotalRowsHeight - visibleHeight);

                    containerState.ContentScrollY =
                        Math.Clamp(containerState.ContentScrollY, 0f, maxScroll);

                    consumed = true;
                    break;
                }

                rowY += rowHeight + RowSpacing;
            }

            if (!consumed && IsContentHovered(bounds))
            {
                containerState.ContentScrollY -= wheel * SCROLL_WHEEL_SPEED;

                float maxScroll =
                    Math.Max(0f, containerState.LastTotalRowsHeight - visibleHeight);

                containerState.ContentScrollY =
                    Math.Clamp(containerState.ContentScrollY, 0f, maxScroll);
            }
        }

        //
        // ------------------------------------------------------------------
        // 5. DRAW ROWS
        // ------------------------------------------------------------------
        //

        float currentRowY = bounds.Y + RowPadding - containerState.ContentScrollY;

        for (int ri = 0; ri < RowCount; ri++)
        {
            float rowHeight = ResolveRowHeight(ri);

            Rectangle rowRect = new Rectangle(
                (int)bounds.X,
                (int)currentRowY,
                (int)bounds.Width,
                (int)rowHeight
            );

            currentRowY += rowHeight + RowSpacing;

            ScissorStack.Push(rowRect);

            float visibleRowStartX = rowRect.X + RowPadding;
            float visibleRowWidth = rowRect.Width - RowPadding * 2f;

            var rowList = RowsContents[ri];
            var rstate = rowStates[ri];

            //
            // ----- Measure children
            //

            float totalContentWidth = 0f;
            List<float> measuredWidths = new List<float>(rowList.Count);

            for (int c = 0; c < rowList.Count; c++)
            {
                var child = rowList[c];

                var childSize = child.GetSize(
                    new Rectangle(0, 0, bounds.Width, (int)Math.Max(0f, rowHeight - RowPadding * 2f))
                );

                float width = Math.Max(0f, childSize.X);

                measuredWidths.Add(width);
                totalContentWidth += width;

                if (c < rowList.Count - 1)
                    totalContentWidth += RowPadding;
            }

            rstate.LastTotalContentWidth = totalContentWidth;
            rstate.IsScrollbarVisible = totalContentWidth > visibleRowWidth;

            if (!rstate.IsScrollbarVisible)
                rstate.ContentScrollX = 0f;

            //
            // ----- Horizontal wheel scroll
            //

            float rowWheel = Input.ScrollDelta;

            if (Math.Abs(rowWheel) > 0.001f && IsContentHovered(rowRect))
            {
                rstate.ContentScrollX -= rowWheel * SCROLL_WHEEL_SPEED;

                float maxScroll =
                    Math.Max(0f, totalContentWidth - visibleRowWidth);

                rstate.ContentScrollX =
                    Math.Clamp(rstate.ContentScrollX, 0f, maxScroll);
            }

            //
            // ----- Draw children
            //

            float contentX = visibleRowStartX - rstate.ContentScrollX;

            for (int c = 0; c < rowList.Count; c++)
            {
                var child = rowList[c];

                float childWidth = measuredWidths[c];
                float childHeight = Math.Max(0f, rowHeight - RowPadding * 2f);

                Rectangle childBounds = new Rectangle(
                    (int)contentX,
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
                    if (child.IsHovered)
                        child.OnHoverEnd();

                    child.IsHovered = false;
                }

                child.InternalDraw(childBounds);

                contentX += childWidth + RowPadding;
            }

            ScissorStack.Pop();
        }

        //
        // ------------------------------------------------------------------
        // 6. FINAL CLAMP + SCROLL PROGRESS
        // ------------------------------------------------------------------
        //

        float maxContainerScroll =
            Math.Max(0f, containerState.LastTotalRowsHeight - visibleHeight);

        containerState.ContentScrollY =
            Math.Clamp(containerState.ContentScrollY, 0f, maxContainerScroll);

        containerState.ScrollProgress =
            (maxContainerScroll <= 0f)
                ? 0f
                : containerState.ContentScrollY / maxContainerScroll;
    }

    protected internal override bool WantsScroll(Rectangle bounds, float wheelDelta)
    {
        // only consider when mouse is over the whole rows area
        if (!IsContentHovered(bounds))
            return false;

        float visibleStartY = bounds.Y + RowPadding;
        float visibleHeight = bounds.Height - RowPadding * 2f;

        // compute total rows height (same calculation as Draw)
        float totalRowsHeight = 0f;
        for (int i = 0; i < RowCount; i++)
            totalRowsHeight += ResolveRowHeight(i);
        totalRowsHeight += RowSpacing * Math.Max(0, RowCount - 1);

        // find the row under the mouse and ask its children first
        float checkY = visibleStartY - containerState.ContentScrollY;
        for (int ri = 0; ri < RowCount; ri++)
        {
            float rowH = ResolveRowHeight(ri);
            Rectangle rowRect = new Rectangle(bounds.X, (int)checkY, bounds.Width, (int)rowH);

            if (IsContentHovered(rowRect))
            {
                var rowList = (ri < RowsContents.Count) ? RowsContents[ri] : new List<UIContent>();
                var rstate = rowStates[ri];

                float visibleRowStartX = rowRect.X + RowPadding;
                float visibleRowWidth = rowRect.Width - RowPadding * 2f;
                float contentWidth = visibleRowWidth;
                if (rstate.IsScrollbarVisible)
                    contentWidth = Math.Max(0f, contentWidth - (rstate.ScrollbarCurrentThickness + RowPadding));

                float contentOffsetX = visibleRowStartX - rstate.ContentScrollX;

                for (int c = 0; c < rowList.Count; c++)
                {
                    var child = rowList[c];
                    var childSize = child.GetSize(new Rectangle(0, 0, int.MaxValue, (int)Math.Max(0f, rowH - RowPadding * 2f)));
                    float childWidth = Math.Max(0f, childSize.X);
                    Rectangle childBounds = new Rectangle((int)contentOffsetX, rowRect.Y + (int)RowPadding, (int)childWidth, (int)Math.Max(0f, rowH - RowPadding * 2f));

                    if (child.WantsScroll(childBounds, wheelDelta))
                        return true;

                    contentOffsetX += childWidth + RowPadding;
                }

                // no child wanted the scroll — stop searching rows (we only check the hovered row)
                break;
            }

            checkY += rowH + RowSpacing;
        }

        // if no child consumed the wheel and vertical scrolling is allowed, claim it if the container can scroll
        if (AllowVerticalScroll && totalRowsHeight > visibleHeight)
            return true;

        return false;
    }

    // small helper for lerp
    private static float Lerp(float a, float b, float t) => a + (b - a) * t;


    public IUIContainer AddContent(UIContent content)
    {
        AddToRow(0, content);
        return this;
    }

    public IUIContainer AddContent(UIContent content, int index)
    {
        if (content == null) return this;

        EnsureRows(RowCount);

        var row = RowsContents[0];
        int idx = Math.Clamp(index, 0, row.Count);
        row.Insert(idx, content);

        content.Owner = this;
        if (setupCalled)
            content.Setup();

        return this;
    }

    public IUIContainer AddContent(UIContent reference, UIContent contentToAdd)
    {
        if (reference == null || contentToAdd == null) return this;

        for (int r = 0; r < RowsContents.Count; r++)
        {
            var row = RowsContents[r];
            int idx = row.IndexOf(reference);
            if (idx >= 0)
            {
                // insert after reference (parity with UIColumns)
                int insertIdx = Math.Clamp(idx + 1, 0, row.Count);
                row.Insert(insertIdx, contentToAdd);

                contentToAdd.Owner = this;
                if (setupCalled)
                    contentToAdd.Setup();

                return this;
            }
        }

        // fallback
        AddToRow(0, contentToAdd);
        return this;
    }

    public IUIContainer AddContent(UIContent reference, UIContent contentToAdd, int index)
    {
        if (reference == null || contentToAdd == null) return this;

        for (int r = 0; r < RowsContents.Count; r++)
        {
            var row = RowsContents[r];
            int refIdx = row.IndexOf(reference);
            if (refIdx >= 0)
            {
                // If index == -1, append to the end of the referenced row
                if (index == -1)
                {
                    row.Add(contentToAdd);
                }
                else
                {
                    int insertIdx = Math.Clamp(index, 0, row.Count);
                    row.Insert(insertIdx, contentToAdd);
                }

                contentToAdd.Owner = this;
                if (setupCalled)
                    contentToAdd.Setup();

                return this;
            }
        }

        // Reference not found: fallback to first row behavior.
        var fallbackRow = RowsContents[0];
        if (index == -1)
            fallbackRow.Add(contentToAdd);
        else
            fallbackRow.Insert(Math.Clamp(index, 0, fallbackRow.Count), contentToAdd);

        contentToAdd.Owner = this;
        if (setupCalled)
            contentToAdd.Setup();

        return this;
    }

    public void RemoveContent(UIContent element)
    {
        if (element == null) return;

        for (int r = 0; r < RowsContents.Count; r++)
        {
            var row = RowsContents[r];
            if (row.Remove(element))
            {
                element.OnOwnerClosing();
                return;
            }
        }
    }

    public void AddAll(List<UIContent> contents)
    {
        if (contents == null) return;

        // preserve order: insert reverse so first item in list ends up first in row
        for (int i = contents.Count - 1; i >= 0; i--)
        {
            UIContent content = contents[i];
            AddToRow(0, content);
        }
    }

    public void AddAll(UIContent reference, List<UIContent> contents)
    {
        if (reference == null || contents == null) return;

        for (int r = 0; r < RowsContents.Count; r++)
        {
            var row = RowsContents[r];
            int idx = row.IndexOf(reference);
            if (idx >= 0)
            {
                for (int i = contents.Count - 1; i >= 0; i--)
                {
                    UIContent content = contents[i];
                    // append to the row (parity with UIColumns behavior)
                    row.Add(content);
                    content.Owner = this;
                    if (setupCalled)
                        content.Setup();
                }
                return;
            }
        }

        // fallback to adding to first row
        AddAll(contents);
    }

    public int GetContentIndex(UIContent content)
    {
        if (content == null) return -1;

        for (int r = 0; r < RowsContents.Count; r++)
        {
            var row = RowsContents[r];
            int idx = row.IndexOf(content);
            if (idx >= 0)
                return idx;
        }
        return -1;
    }
    void IUIContainer.Close() => Owner.Close();
}

