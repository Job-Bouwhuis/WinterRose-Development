﻿using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.Utility;

namespace WinterRose.ForgeWarden.UserInterface;
public class UIColumns : UIContent
{
    // configuration
    public const int DEFAULT_COLUMN_COUNT = 2;
    public const float DEFAULT_COLUMN_HEIGHT = 160f; // logical default height when no constraint is given
    public int ColumnCount { get; set; } = DEFAULT_COLUMN_COUNT;
    /// <summary>
    /// If > 0 each column will use this fixed width. If 0 (default) columns auto-scale to available width.
    /// </summary>
    public float FixedColumnWidth { get; set; } = 0f;
    public float ColumnSpacing { get; set; } = 6f;
    public float ColumnPadding { get; set; } = UIConstants.CONTENT_PADDING;

    // scrollbar animation constants (kept local so subcontainers behave independently)
    private const float SCROLLBAR_HOVER_DELAY = 0.25f;
    private const float SCROLLBAR_COLLAPSED_WIDTH = 6f;
    private const float SCROLLBAR_EXPANDED_WIDTH = 12f;
    private const float SCROLL_WHEEL_SPEED = 24f;
    private const float SCROLLBAR_ANIM_DURATION = 0.12f;

    // columns content storage
    public List<List<UIContent>> ColumnsContents { get; } = new();

    // per-column runtime state
    private class ColumnState
    {
        public float ContentScrollY = 0f;
        public bool IsScrollbarVisible = false;
        public float ScrollbarCurrentWidth = SCROLLBAR_COLLAPSED_WIDTH;
        public float ScrollbarAnimProgress = 0f;
        public float ScrollbarHoverTimer = 0f;
        public bool ScrollbarHoverTarget = false;
        public bool IsScrollDragging = false;
        public float LastTotalContentHeight = 0f;
    }

    private readonly List<ColumnState> columnStates = new();

    public UIColumns()
    {
        EnsureColumns(ColumnCount);
    }

    protected internal override void Setup()
    {
        base.Setup();
        EnsureColumns(ColumnCount);
    }

    private void EnsureColumns(int count)
    {
        if (count <= 0) count = DEFAULT_COLUMN_COUNT;

        // resize column content lists
        while (ColumnsContents.Count < count)
            ColumnsContents.Add(new List<UIContent>());

        // resize states
        while (columnStates.Count < count)
            columnStates.Add(new ColumnState());

        ColumnCount = count;
    }

    /// <summary>
    /// Add content to a specific column index (0-based). If index is out of range, it will be clamped.
    /// </summary>
    public void AddToColumn(int columnIndex, UIContent content)
    {
        if (content == null) return;
        EnsureColumns(columnIndex + 1);
        int idx = Math.Clamp(columnIndex, 0, ColumnCount - 1);
        ColumnsContents[idx].Add(content);
        content.owner = owner;
        content.Setup();
    }

    public void ClearColumn(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= ColumnsContents.Count) return;
        foreach (var c in ColumnsContents[columnIndex])
            c.OnOwnerClosing();
        ColumnsContents[columnIndex].Clear();
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        // width uses availableArea.Width
        float width = availableArea.Width;

        // choose sensible default height independent of available area if caller gives infinite height:
        // We'll measure heights of column contents using the column width each would get.
        float columnWidth = (FixedColumnWidth > 0f)
            ? FixedColumnWidth
            : (width - ColumnSpacing * (ColumnCount - 1)) / Math.Max(1, ColumnCount);

        float maxColumnHeight = 0f;
        for (int c = 0; c < ColumnCount; c++)
        {
            float colHeight = 0f;
            if (c < ColumnsContents.Count)
            {
                foreach (var child in ColumnsContents[c])
                {
                    colHeight += child.GetHeight((int)columnWidth);
                    colHeight += ColumnPadding;
                }
            }
            maxColumnHeight = Math.Max(maxColumnHeight, colHeight);
        }

        // If there are no children, give a default logical height
        if (maxColumnHeight <= 0f)
            maxColumnHeight = DEFAULT_COLUMN_HEIGHT;

        return new Vector2(width, maxColumnHeight);
    }

    protected internal override float GetHeight(float maxWidth)
    {
        return GetSize(new Rectangle(0, 0, (int)maxWidth, int.MaxValue)).Y;
    }

    protected internal override void Update()
    {
        // Update children logic (non-visual updates)
        foreach (var column in ColumnsContents)
        {
            foreach (var child in column)
                child.Update();
        }
    }

    protected internal override void OnOwnerClosing()
    {
        foreach (var column in ColumnsContents)
            foreach (var child in column)
                child.OnOwnerClosing();
    }

    protected internal override void OnHover()
    {
        // Delegate hover checks to children when Draw/Update set last-known rectangles.
        // Nothing specific here; children will receive hover in Draw's per-column hit test.
        base.OnHover();
    }

    protected internal override void OnHoverEnd()
    {
        foreach (var column in ColumnsContents)
            foreach (var child in column)
                child.OnHoverEnd();
    }

    protected internal override void OnContentClicked(MouseButton button)
    {
        // If a child was hovered earlier it will have received click in Draw's hit testing.
        base.OnContentClicked(button);
    }

    protected internal override void OnClickedOutsideOfContent(MouseButton button)
    {
        // Propagate to children so they can respond as needed
        foreach (var column in ColumnsContents)
            foreach (var child in column)
                child.OnClickedOutsideOfContent(button);
    }

    protected override void Draw(Rectangle bounds)
    {
        if (ColumnCount <= 0) return;

        EnsureColumns(ColumnCount);

        float totalSpacing = ColumnSpacing * (ColumnCount - 1);
        float autoColumnWidth = FixedColumnWidth > 0f
            ? FixedColumnWidth
            : Math.Max(0f, (bounds.Width - totalSpacing) / ColumnCount);

        // For each column compute its rect and handle scissor + per-column scroll/hover/interaction
        for (int ci = 0; ci < ColumnCount; ci++)
        {
            float colX = bounds.X + ci * (autoColumnWidth + ColumnSpacing);
            float colY = bounds.Y;
            float colW = autoColumnWidth;
            float colH = bounds.Height;

            Rectangle colRect = new Rectangle((int)colX, (int)colY, (int)colW, (int)colH);

            // push scissor so column content is clipped
            ScissorStack.Push(colRect);

            // compute visible content area inside column (respect padding and possible title bar not relevant here)
            float visibleStartY = colRect.Y + ColumnPadding;
            float visibleHeight = colRect.Height - ColumnPadding * 2f;
            float availableContentWidthCandidate = Math.Max(0f, colRect.Width - ColumnPadding * 2f);

            // compute total content height for this column
            float totalContentHeight = 0f;
            var columnList = (ci < ColumnsContents.Count) ? ColumnsContents[ci] : new List<UIContent>();
            foreach (var content in columnList)
            {
                totalContentHeight += content.GetHeight((int)availableContentWidthCandidate);
                totalContentHeight += ColumnPadding;
            }

            var state = columnStates[ci];

            // determine scrollbar visibility and recalc if visible (reserve space)
            if (totalContentHeight > visibleHeight)
            {
                state.IsScrollbarVisible = true;
                float reserved = state.ScrollbarCurrentWidth + ColumnPadding;
                float availableContentWidth = Math.Max(0f, availableContentWidthCandidate - reserved);

                totalContentHeight = 0f;
                foreach (var content in columnList)
                {
                    totalContentHeight += content.GetHeight((int)availableContentWidth);
                    totalContentHeight += ColumnPadding;
                }

                state.LastTotalContentHeight = totalContentHeight;
            }
            else
            {
                state.IsScrollbarVisible = false;
                state.LastTotalContentHeight = totalContentHeight;
            }

            // handle hover/anim for scrollbar (local copy of earlier logic)
            var mouse = Input.MousePosition;
            float trackCenterX = colRect.X + colRect.Width - ColumnPadding - (state.ScrollbarCurrentWidth / 2f);
            float trackTopY = visibleStartY;
            float trackBottomY = visibleStartY + visibleHeight;

            bool nearX = Math.Abs(mouse.X - trackCenterX) <= (state.ScrollbarCurrentWidth / 2f);
            bool withinY = mouse.Y >= trackTopY && mouse.Y <= trackBottomY;
            bool isHoveringScrollbar = state.IsScrollDragging || (state.IsScrollbarVisible && nearX && withinY);
            if (isHoveringScrollbar)
            {
                state.ScrollbarHoverTimer += Time.deltaTime;
                if (state.ScrollbarHoverTimer >= SCROLLBAR_HOVER_DELAY)
                {
                    state.ScrollbarHoverTimer = SCROLLBAR_HOVER_DELAY;
                    state.ScrollbarHoverTarget = true;
                }
            }
            else
            {
                state.ScrollbarHoverTimer -= Time.deltaTime;
                if (state.ScrollbarHoverTimer <= 0f)
                {
                    state.ScrollbarHoverTimer = 0f;
                    state.ScrollbarHoverTarget = false;
                }
            }

            float target = state.ScrollbarHoverTarget ? 1f : 0f;
            if (state.ScrollbarAnimProgress != target)
            {
                float delta = Time.deltaTime / Math.Max(0.0001f, SCROLLBAR_ANIM_DURATION);
                if (target > state.ScrollbarAnimProgress)
                    state.ScrollbarAnimProgress = Math.Min(1f, state.ScrollbarAnimProgress + delta);
                else
                    state.ScrollbarAnimProgress = Math.Max(0f, state.ScrollbarAnimProgress - delta);
            }

            float eased = Curves.Linear.Evaluate(state.ScrollbarAnimProgress);
            state.ScrollbarCurrentWidth = Lerp(SCROLLBAR_COLLAPSED_WIDTH, SCROLLBAR_EXPANDED_WIDTH, eased);

            // handle mouse wheel for column only when hovering that column
            bool isColumnHovered = IsContentHovered(colRect);
            if (isColumnHovered)
            {
                float wheel = Raylib_cs.Raylib.GetMouseWheelMove();
                if (Math.Abs(wheel) > 0.001f)
                {
                    state.ContentScrollY -= wheel * SCROLL_WHEEL_SPEED;
                    // clamp
                    state.ContentScrollY = Math.Clamp(state.ContentScrollY, 0f, Math.Max(0f, state.LastTotalContentHeight - visibleHeight));
                }
            }

            // compute content offset and draw children in column
            float contentOffsetY = visibleStartY - state.ContentScrollY;
            float contentX = colRect.X + ColumnPadding;
            float contentWidth = availableContentWidthCandidate;
            if (state.IsScrollbarVisible)
                contentWidth = Math.Max(0f, availableContentWidthCandidate - (state.ScrollbarCurrentWidth + ColumnPadding));

            for (int ciChild = 0; ciChild < columnList.Count; ciChild++)
            {
                var content = columnList[ciChild];
                float contentHeight = content.GetHeight((int)contentWidth);

                Rectangle contentBounds = new Rectangle(
                    (int)contentX,
                    (int)contentOffsetY,
                    (int)contentWidth,
                    (int)contentHeight
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

                // draw child using its normal draw path
                content.InternalDraw(contentBounds);

                contentOffsetY += contentHeight + ColumnPadding;
            }

            // pop scissor for this column
            ScissorStack.Pop();
        }
    }

    // small helper for lerp
    private static float Lerp(float a, float b, float t) => a + (b - a) * t;

    public void RemoveColumn(int index)
    {
        // Validate index
        if (index < 0 || index >= ColumnsContents.Count)
            return;

        // Notify children the owner is closing before removal
        foreach (var c in ColumnsContents[index])
            c.OnOwnerClosing();

        // Remove the column contents and its state
        ColumnsContents.RemoveAt(index);
        columnStates.RemoveAt(index);

        // Update the count
        ColumnCount = ColumnsContents.Count;

        // Ensure we always have at least one column
        if (ColumnCount == 0)
            EnsureColumns(DEFAULT_COLUMN_COUNT);
    }
}

