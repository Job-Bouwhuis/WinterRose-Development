using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.Utility;

namespace WinterRose.ForgeWarden.UserInterface;
public class UIGrid : UIContent
{
    // configuration
    public const int DEFAULT_ROW_COUNT = 2;
    public const int DEFAULT_COLUMN_COUNT = 2;
    public const float DEFAULT_ROW_HEIGHT = 80f;    // used when FixedRowHeight == 0 (auto fallback)
    public const float DEFAULT_COLUMN_WIDTH = 160f; // used when FixedColumnWidth == 0 (auto fallback)

    public int RowCount { get; private set; } = DEFAULT_ROW_COUNT;
    public int ColumnCount { get; private set; } = DEFAULT_COLUMN_COUNT;

    /// <summary>
    /// If > 0 each column will use this fixed width. If 0 (default) columns auto-scale to available width.
    /// </summary>
    public float FixedColumnWidth { get; set; } = 0f;

    /// <summary>
    /// If > 0 each row will use this fixed height. If 0 (default) rows auto-scale by measuring children and falling back to DEFAULT_ROW_HEIGHT.
    /// </summary>
    public float FixedRowHeight { get; set; } = 0f;

    public float ColumnSpacing { get; set; } = 6f;
    public float RowSpacing { get; set; } = 6f;
    public float CellPadding { get; set; } = UIConstants.CONTENT_PADDING;

    // scrollbar animation constants
    private const float SCROLLBAR_HOVER_DELAY = 0.25f;
    private const float SCROLLBAR_COLLAPSED_THICKNESS = 6f;
    private const float SCROLLBAR_EXPANDED_THICKNESS = 12f;
    private const float SCROLL_WHEEL_SPEED = 24f;
    private const float SCROLLBAR_ANIM_DURATION = 0.12f;

    // grid storage: row -> list of cells (each cell holds a UIContent or null)
    public List<List<UIContent?>> GridContents { get; } = new();

    public bool ShowCellBorders { get; set; } = true;
    public float CellBorderThickness { get; set; } = 1f; // thickness in pixels (will be rounded when drawing)
    public Color CellBorderColor { get; set; } = Color.White;

    // per-cell runtime state
    private class CellState
    {
        public float ContentScrollY = 0f;
        public bool IsScrollbarVisible = false;
        public float ScrollbarCurrentThickness = SCROLLBAR_COLLAPSED_THICKNESS;
        public float ScrollbarAnimProgress = 0f;
        public float ScrollbarHoverTimer = 0f;
        public bool ScrollbarHoverTarget = false;
        public bool IsScrollDragging = false;
        public float LastTotalContentHeight = 0f;
    }

    private readonly List<List<CellState>> cellStates = new();

    public UIGrid()
    {
        EnsureGrid(RowCount, ColumnCount);
    }

    protected internal override void Setup()
    {
        base.Setup();
        EnsureGrid(RowCount, ColumnCount);
    }

    private void EnsureGrid(int rows, int columns)
    {
        if (rows <= 0) rows = DEFAULT_ROW_COUNT;
        if (columns <= 0) columns = DEFAULT_COLUMN_COUNT;

        // Resize GridContents to requested row count
        while (GridContents.Count < rows)
            GridContents.Add(new List<UIContent?>());

        // Ensure each row has exactly 'columns' entries
        for (int r = 0; r < GridContents.Count; r++)
        {
            var row = GridContents[r];
            while (row.Count < columns) row.Add(null);
        }

        // Build a new cellStates matrix that matches the GridContents shape,
        // preserving existing CellState entries where possible.
        var newStates = new List<List<CellState>>(rows);
        for (int r = 0; r < rows; r++)
        {
            var newRowStates = new List<CellState>(columns);
            List<CellState>? oldRow = (r < cellStates.Count) ? cellStates[r] : null;

            for (int c = 0; c < columns; c++)
            {
                if (oldRow != null && c < oldRow.Count)
                    newRowStates.Add(oldRow[c]); // preserve
                else
                    newRowStates.Add(new CellState()); // create fresh
            }

            newStates.Add(newRowStates);
        }

        // Replace the old states with the new one
        cellStates.Clear();
        cellStates.AddRange(newStates);

        // update counts
        RowCount = GridContents.Count;
        ColumnCount = GridContents.Count > 0 ? GridContents[0].Count : 0;

        // safety fallback
        if (RowCount == 0)
            EnsureGrid(DEFAULT_ROW_COUNT, Math.Max(1, columns));
        if (ColumnCount == 0)
            EnsureGrid(Math.Max(1, rows), DEFAULT_COLUMN_COUNT);
    }

    public void AddToCell(int rowIndex, int columnIndex, UIContent content)
    {
        if (content == null) return;

        EnsureGrid(rowIndex + 1, columnIndex + 1);

        int r = Math.Clamp(rowIndex, 0, RowCount - 1);
        int c = Math.Clamp(columnIndex, 0, ColumnCount - 1);

        GridContents[r][c] = content;

        content.Owner = Owner;
        content.Setup();
    }

    public void ClearCell(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0 || rowIndex >= RowCount) return;
        if (columnIndex < 0 || columnIndex >= ColumnCount) return;

        var content = GridContents[rowIndex][columnIndex];
        if (content != null)
            content.OnOwnerClosing();

        GridContents[rowIndex][columnIndex] = null;
        cellStates[rowIndex][columnIndex] = new CellState();
    }

    public void RemoveRow(int index)
    {
        if (index < 0 || index >= RowCount) return;

        // notify and remove
        foreach (var cell in GridContents[index])
            cell?.OnOwnerClosing();

        GridContents.RemoveAt(index);
        cellStates.RemoveAt(index);

        RowCount = GridContents.Count;
        if (RowCount == 0)
            EnsureGrid(DEFAULT_ROW_COUNT, ColumnCount);
    }

    public void RemoveColumn(int index)
    {
        if (index < 0 || index >= ColumnCount) return;

        for (int r = 0; r < GridContents.Count; r++)
        {
            var cell = GridContents[r][index];
            cell?.OnOwnerClosing();
            GridContents[r].RemoveAt(index);
            cellStates[r].RemoveAt(index);
        }

        ColumnCount = GridContents.Count > 0 ? GridContents[0].Count : 0;
        if (ColumnCount == 0)
            EnsureGrid(RowCount, DEFAULT_COLUMN_COUNT);
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        float width = availableArea.Width;

        // compute column widths
        float totalColumnSpacing = ColumnSpacing * Math.Max(0, ColumnCount - 1);
        float columnWidth = FixedColumnWidth > 0f
            ? FixedColumnWidth
            : Math.Max(0f, (width - totalColumnSpacing) / Math.Max(1, ColumnCount));

        // compute row heights
        float totalRowsHeight = 0f;
        for (int r = 0; r < RowCount; r++)
        {
            float rowHeight = FixedRowHeight > 0f ? FixedRowHeight : 0f;

            if (FixedRowHeight <= 0f)
            {
                // measure largest child in this row to determine row height
                float measured = 0f;
                for (int c = 0; c < ColumnCount; c++)
                {
                    var child = GridContents[r][c];
                    if (child == null) continue;

                    var size = child.GetSize(new Rectangle(0, 0, (int)Math.Max(0f, columnWidth - CellPadding * 2f), int.MaxValue));
                    measured = Math.Max(measured, size.Y);
                }
                rowHeight = Math.Max(DEFAULT_ROW_HEIGHT, measured + CellPadding * 2f);
            }

            totalRowsHeight += rowHeight;
            if (r < RowCount - 1) totalRowsHeight += RowSpacing;
        }

        // if no content, fallback to a sensible grid size
        if (RowCount == 0 || ColumnCount == 0)
            return new Vector2(width, DEFAULT_ROW_HEIGHT);

        return new Vector2(width, totalRowsHeight);
    }

    protected internal override float GetHeight(float maxWidth)
    {
        return GetSize(new Rectangle(0, 0, (int)maxWidth, int.MaxValue)).Y;
    }

    protected internal override void Update()
    {
        for (int r = 0; r < RowCount; r++)
        {
            for (int c = 0; c < ColumnCount; c++)
            {
                var child = GridContents[r][c];
                child?.Update();
            }
        }
    }

    protected internal override void OnOwnerClosing()
    {
        for (int r = 0; r < RowCount; r++)
            for (int c = 0; c < ColumnCount; c++)
                GridContents[r][c]?.OnOwnerClosing();
    }

    protected internal override void OnHoverEnd()
    {
        for (int r = 0; r < RowCount; r++)
            for (int c = 0; c < ColumnCount; c++)
                GridContents[r][c]?.OnHoverEnd();
    }

    protected internal override void OnClickedOutsideOfContent(MouseButton button)
    {
        for (int r = 0; r < RowCount; r++)
            for (int c = 0; c < ColumnCount; c++)
                GridContents[r][c]?.OnClickedOutsideOfContent(button);
    }

    protected override void Draw(Rectangle bounds)
    {
        if (RowCount <= 0 || ColumnCount <= 0) return;
        EnsureGrid(RowCount, ColumnCount);

        float totalColumnSpacing = ColumnSpacing * Math.Max(0, ColumnCount - 1);
        float columnWidth = FixedColumnWidth > 0f
            ? FixedColumnWidth
            : Math.Max(0f, (bounds.Width - totalColumnSpacing) / Math.Max(1, ColumnCount));

        // compute each row height (respect FixedRowHeight)
        List<float> rowHeights = new();
        for (int r = 0; r < RowCount; r++)
        {
            float rowHeight = FixedRowHeight > 0f ? FixedRowHeight : 0f;

            if (FixedRowHeight <= 0f)
            {
                float measured = 0f;
                for (int c = 0; c < ColumnCount; c++)
                {
                    var child = GridContents[r][c];
                    if (child == null) continue;

                    var size = child.GetSize(new Rectangle(0, 0, (int)Math.Max(0f, columnWidth - CellPadding * 2f), int.MaxValue));
                    measured = Math.Max(measured, size.Y);
                }

                rowHeight = Math.Max(DEFAULT_ROW_HEIGHT, measured + CellPadding * 2f);
            }

            rowHeights.Add(rowHeight);
        }

        // draw cells: position each cell rect, push scissor, handle per-cell vertical scrolling and events
        float y = bounds.Y;
        for (int r = 0; r < RowCount; r++)
        {
            float rowHeight = rowHeights[r];

            float x = bounds.X;
            for (int c = 0; c < ColumnCount; c++)
            {
                float cellX = x;
                float cellY = y;
                float cellW = columnWidth;
                float cellH = rowHeight;

                Rectangle cellRect = new Rectangle((int)cellX, (int)cellY, (int)cellW, (int)cellH);

                // compute border inset so content isn't drawn under the border
                float borderInset = (ShowCellBorders && CellBorderThickness > 0f) ? CellBorderThickness : 0f;
                Rectangle innerRect = new Rectangle(
                    (int)Math.Round(cellRect.X + borderInset),
                    (int)Math.Round(cellRect.Y + borderInset),
                    (int)Math.Max(0f, Math.Round(cellRect.Width - borderInset * 2f)),
                    (int)Math.Max(0f, Math.Round(cellRect.Height - borderInset * 2f))
                );

                // push scissor for inner area (so border remains visible outside the scissor if needed)
                ScissorStack.Push(innerRect);

                // inside cell metrics use innerRect instead of cellRect
                float visibleStartY = innerRect.Y + CellPadding;
                float visibleHeight = innerRect.Height - CellPadding * 2f;
                float availableContentWidthCandidate = Math.Max(0f, innerRect.Width - CellPadding * 2f);

                var content = GridContents[r][c];
                var state = cellStates[r][c];

                // measure content height
                float contentHeight = 0f;
                if (content != null)
                {
                    contentHeight = content.GetHeight((int)availableContentWidthCandidate);
                }

                // scrollbar visibility
                if (contentHeight > visibleHeight)
                {
                    state.IsScrollbarVisible = true;
                    float reserved = state.ScrollbarCurrentThickness + CellPadding;
                    float availableContentWidth = Math.Max(0f, availableContentWidthCandidate - reserved);

                    // recalc accurate total content height with reserved width
                    state.LastTotalContentHeight = content?.GetHeight((int)availableContentWidth) ?? 0f;
                }
                else
                {
                    state.IsScrollbarVisible = false;
                    state.LastTotalContentHeight = contentHeight;
                    state.ContentScrollY = Math.Clamp(state.ContentScrollY, 0f, Math.Max(0f, state.LastTotalContentHeight - visibleHeight));
                }

                // handle hover/anim for cell scrollbar (uses innerRect coordinates where appropriate)
                var mouse = Input.MousePosition;
                float trackCenterX = innerRect.X + innerRect.Width - CellPadding - (state.ScrollbarCurrentThickness / 2f);
                float trackTopY = visibleStartY;
                float trackBottomY = visibleStartY + visibleHeight;

                bool nearX = Math.Abs(mouse.X - trackCenterX) <= (state.ScrollbarCurrentThickness / 2f);
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
                state.ScrollbarCurrentThickness = Lerp(SCROLLBAR_COLLAPSED_THICKNESS, SCROLLBAR_EXPANDED_THICKNESS, eased);

                // per-cell wheel when hovering the inner area
                bool isCellHovered = IsContentHovered(innerRect);
                if (isCellHovered && state.IsScrollbarVisible)
                {
                    float wheel = Raylib_cs.Raylib.GetMouseWheelMove();
                    if (Math.Abs(wheel) > 0.001f)
                    {
                        state.ContentScrollY -= wheel * SCROLL_WHEEL_SPEED;
                        state.ContentScrollY = Math.Clamp(state.ContentScrollY, 0f, Math.Max(0f, state.LastTotalContentHeight - visibleHeight));
                    }
                }

                // draw content with content offset inside innerRect
                float contentOffsetY = visibleStartY - state.ContentScrollY;
                if (content != null)
                {
                    Rectangle contentBounds = new Rectangle(
                        (int)(innerRect.X + CellPadding),
                        (int)contentOffsetY,
                        (int)availableContentWidthCandidate,
                        (int)state.LastTotalContentHeight
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

                    content.InternalDraw(contentBounds);
                }

                // pop scissor for inner area
                ScissorStack.Pop();

                // draw border around the original cellRect (outside scissor) if requested
                if (ShowCellBorders && CellBorderThickness > 0f)
                {
                    int thickness = Math.Max(1, (int)Math.Round(CellBorderThickness));
                    Raylib_cs.Raylib.DrawRectangleLinesEx(cellRect, thickness, CellBorderColor);
                }

                x += columnWidth;
                if (c < ColumnCount - 1) x += ColumnSpacing;
            }

            y += rowHeight;
            if (r < RowCount - 1) y += RowSpacing;
        }

        // clamp all cell scrolls after layout changes
        for (int r = 0; r < RowCount; r++)
            for (int c = 0; c < ColumnCount; c++)
            {
                var s = cellStates[r][c];
                s.ContentScrollY = Math.Clamp(s.ContentScrollY, 0f, Math.Max(0f, s.LastTotalContentHeight - (rowHeights[r] - CellPadding * 2f)));
            }
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
