using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden.TextRendering;

namespace WinterRose.ForgeWarden.UserInterface;

public class UIRadioButtons : UIContent
{
    // Layout constants (uppercase style)
    private const float PADDING_X = 8f;
    private const float PADDING_Y = 6f;
    private const float SPACING = 8f;

    private const int DOT_SIZE = 14;              // outer dot diameter
    private const int LABEL_BASE_SIZE = 12;
    private const float ANIMATION_SPEED = 10f;    // how fast the dot fill animates

    private int labelFontSize;

    private int maxSelected = 1;
    public int MaxSelected
    {
        get => maxSelected;
        set
        {
            if (value < 1) value = 1;
            if (maxSelected == value) return;
            maxSelected = value;

            // If we currently have more selections than the new max, trim them
            var selected = GetSelectedIndices();
            if (selected.Count > maxSelected)
            {
                var keep = selected.Take(maxSelected).ToList();
                SetSelectedIndices(keep, true);
            }
        }
    }

    // Internal option representation (no external UIContent used)
    private class Option
    {
        public RichText Text;
        public bool Selected;
        public float AnimationProgress; // 0..1
        public bool IsHovered;

        public Option(RichText text, bool initial)
        {
            Text = text;
            Selected = initial;
            AnimationProgress = initial ? 1f : 0f;
            IsHovered = false;
        }
    }

    // Managed options
    private readonly List<Option> options = new();

    // Layout computed each Draw
    private List<List<Rectangle>> optionColumns = new();

    // Selection callbacks
    public MulticastVoidInvocation<IUIContainer, UIRadioButtons, List<int>> OnSelectionChanged { get; set; } = new();
    public MulticastVoidInvocation<List<int>> OnSelectionChangedBasic { get; set; } = new();

    // Selected index storage
    private int selectedIndex = -1;
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (selectedIndex == value) return;
            SetSelectedIndex(value, true);
        }
    }

    // Guard for programmatic changes
    private bool suppressSelectionCallback = false;

    public UIRadioButtons() { }

    // Add option helpers
    public void AddOption(string text, bool initial = false) => AddOption(RichText.Parse(text), initial);
    public void AddOption(RichText richText, bool initial = false)
    {
        var opt = new Option(richText, initial);
        options.Add(opt);

        // if initial is requested, set selectedIndex accordingly (first initial wins)
        if (initial)
        {
            if (selectedIndex == -1)
                SetSelectedIndex(options.Count - 1, false);
            else
                opt.Selected = false; // keep invariant: only one initial
        }
    }

    // --- Replace existing SetSelectedIndex with this updated method ---
    private void SetSelectedIndex(int index, bool invokeCallbacks)
    {
        if (index < -1 || index >= options.Count) return;

        // If we're in multi-select mode and user is calling SetSelectedIndex,
        // we'll clear others and set this one only (maintains legacy single-index setter behavior).
        selectedIndex = index;

        suppressSelectionCallback = true;
        for (int i = 0; i < options.Count; i++)
            options[i].Selected = (i == selectedIndex);
        suppressSelectionCallback = false;

        if (invokeCallbacks)
            InvokeSelectionCallbacks();
    }

    // --- New helper: return list of selected indices ---
    public List<int> GetSelectedIndices()
    {
        var list = new List<int>();
        for (int i = 0; i < options.Count; i++)
            if (options[i].Selected) list.Add(i);
        return list;
    }

    // --- New: set multiple selected indices (enforces MaxSelected) ---
    public void SetSelectedIndices(IEnumerable<int> indices, bool invokeCallbacks = true)
    {
        indices ??= Array.Empty<int>();
        var filtered = indices.Where(i => i >= 0 && i < options.Count).Distinct().ToList();

        // respect maxSelected: keep earliest indices up to the limit
        filtered = filtered.OrderBy(i => i).Take(maxSelected).ToList();

        suppressSelectionCallback = true;
        for (int i = 0; i < options.Count; i++)
            options[i].Selected = filtered.Contains(i);
        suppressSelectionCallback = false;

        // update selectedIndex (for compatibility) to first selected or -1
        selectedIndex = filtered.Count > 0 ? filtered[0] : -1;

        if (invokeCallbacks)
            InvokeSelectionCallbacks();
    }

    // --- New helper: centralize invoking correct callbacks for single/multi modes ---
    private void InvokeSelectionCallbacks()
    {
        if (maxSelected == 1)
        {
            // legacy single-index callbacks
            OnSelectionChanged?.Invoke(Owner, this, new List<int>(selectedIndex));
            OnSelectionChangedBasic?.Invoke(new List<int>(selectedIndex));
        }
        else
        {
            // multi callbacks with list of indices
            var sel = GetSelectedIndices();
            OnSelectionChanged?.Invoke(Owner, this, sel);
            OnSelectionChangedBasic?.Invoke(sel);
        }
    }

    // Delegation lifecycle (none of the options are UIContent, but keep hooks consistent)
    protected internal override void Setup()
    {
        // nothing needed, but keep for symmetry
    }

    protected override void Update()
    {
        // animate each option's progress toward its target (Selected -> 1, else -> 0)
        float dt = Raylib_cs.Raylib.GetFrameTime();
        for (int i = 0; i < options.Count; i++)
        {
            var opt = options[i];
            float target = opt.Selected ? 1f : 0f;
            if (opt.AnimationProgress < target)
                opt.AnimationProgress = MathF.Min(target, opt.AnimationProgress + ANIMATION_SPEED * dt);
            else if (opt.AnimationProgress > target)
                opt.AnimationProgress = MathF.Max(target, opt.AnimationProgress - ANIMATION_SPEED * dt);
        }
    }

    // Hover handling uses optionColumns computed during Draw; if not computed, do nothing
    protected internal override void OnHover()
    {
        if (optionColumns.SelectMany(c => c).Count() != options.Count)
            return;

        var rects = optionColumns.SelectMany(c => c).ToList();
        int mx = Raylib_cs.Raylib.GetMouseX();
        int my = Raylib_cs.Raylib.GetMouseY();

        for (int i = 0; i < options.Count; i++)
        {
            var r = rects[i];
            bool inside = mx >= r.X && mx <= r.X + r.Width && my >= r.Y && my <= r.Y + r.Height;
            if (inside)
            {
                if (!options[i].IsHovered)
                {
                    options[i].IsHovered = true;
                    // no child.OnHover() call — we own visuals
                }
            }
            else
            {
                if (options[i].IsHovered)
                    options[i].IsHovered = false;
            }
        }
    }

    protected internal override void OnHoverEnd()
    {
        foreach (var o in options)
            o.IsHovered = false;
    }

    protected internal override void OnContentClicked(MouseButton button)
    {
        // Try hovered first
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].IsHovered)
            {
                HandleOptionClick(i, button);
                return;
            }
        }

        // Fallback: check layout rectangles
        int mx = Raylib_cs.Raylib.GetMouseX();
        int my = Raylib_cs.Raylib.GetMouseY();
        foreach (var col in optionColumns)
        {
            for (int j = 0; j < col.Count; j++)
            {
                var rect = col[j];
                if (mx >= rect.X && mx <= rect.X + rect.Width && my >= rect.Y && my <= rect.Y + rect.Height)
                {
                    int flatIndex = optionColumns.TakeWhile(c => c != col).Sum(c => c.Count) + j;
                    if (flatIndex >= 0 && flatIndex < options.Count)
                    {
                        HandleOptionClick(flatIndex, button);
                        return;
                    }
                }
            }
        }
    }

    private void HandleOptionClick(int index, MouseButton button)
    {
        if (index < 0 || index >= options.Count) return;

        if (maxSelected == 1)
        {
            // Single-select behavior (same as before)
            if (selectedIndex == index) return;
            SetSelectedIndex(index, true);
            return;
        }

        // Multi-select mode: toggle if already selected; otherwise attempt to add if under max
        var opt = options[index];
        if (opt.Selected)
        {
            // deselect
            opt.Selected = false;
            // keep selectedIndex in sync (first selected or -1)
            var remaining = GetSelectedIndices();
            selectedIndex = remaining.Count > 0 ? remaining[0] : -1;
            if (!suppressSelectionCallback)
                InvokeSelectionCallbacks();
        }
        else
        {
            var curr = GetSelectedIndices();
            if (curr.Count >= maxSelected)
            {
                // at capacity -> ignore this activation (user must manually deselect first)
                return;
            }

            opt.Selected = true;
            // update selectedIndex to first selected for compatibility
            var now = GetSelectedIndices();
            selectedIndex = now.Count > 0 ? now[0] : -1;
            if (!suppressSelectionCallback)
                InvokeSelectionCallbacks();
        }
    }

    protected internal override void OnClickedOutsideOfContent(MouseButton button)
    {
        // nothing special to do, but exposed for symmetry
    }

    protected internal override void OnOwnerClosing()
    {
        options.Clear();
        optionColumns.Clear();
    }

    // Measurement and layout (very similar to previous behavior, but using our Option objects)
    private Rectangle MeasureOption(Option option, float maxWidth)
    {
        option.Text.FontSize = labelFontSize;
        float textMaxWidth = Math.Max(0f, maxWidth - (DOT_SIZE + SPACING + PADDING_X * 2));
        Rectangle textSize = option.Text.CalculateBounds(textMaxWidth);

        int totalWidth = (int)(DOT_SIZE + SPACING + textSize.Width + PADDING_X * 2);
        int totalHeight = Math.Max(DOT_SIZE, (int)textSize.Height) + (int)PADDING_Y * 2;
        return new Rectangle(0, 0, totalWidth, totalHeight);
    }

    private void UpdateFontSize(float maxWidth)
    {
        float baseScale = maxWidth / UIConstants.TOAST_WIDTH;
        labelFontSize = Math.Clamp((int)(LABEL_BASE_SIZE * baseScale), 12, 28);
    }

    public override Vector2 GetSize(Rectangle availableSize)
    {
        if (options.Count == 0) return new Vector2(0, 0);

        float availW = availableSize.Width;
        float availH = availableSize.Height;

        UpdateFontSize(availW);

        // measure each option conservatively
        List<Rectangle> measured = new();
        for (int i = 0; i < options.Count; i++)
            measured.Add(MeasureOption(options[i], availW));

        // pack vertically into columns (wrap to new column when height exceeded)
        List<float> colWidths = new();
        List<float> colHeights = new();
        List<List<Rectangle>> cols = new();

        float curColHeight = 0f;
        float curColWidth = 0f;
        List<Rectangle> curCol = new();

        for (int i = 0; i < measured.Count; i++)
        {
            var size = measured[i];
            float needed = (curCol.Count == 0) ? size.Height : (SPACING + size.Height);

            if (curCol.Count > 0 && curColHeight + needed > availH && availH > 0)
            {
                cols.Add(curCol);
                colWidths.Add(curColWidth);
                colHeights.Add(curColHeight);

                curCol = new List<Rectangle>();
                curColHeight = 0f;
                curColWidth = 0f;
            }

            curCol.Add(size);
            curColHeight = (curCol.Count == 1) ? size.Height : curColHeight + SPACING + size.Height;
            curColWidth = Math.Max(curColWidth, size.Width);
        }

        if (curCol.Count > 0)
        {
            cols.Add(curCol);
            colWidths.Add(curColWidth);
            colHeights.Add(curColHeight);
        }

        float totalWidth = colWidths.Sum() + SPACING * Math.Max(0, colWidths.Count - 1);
        float totalHeight = colHeights.Count > 0 ? colHeights.Max() : 0f;

        if (availableSize.Height <= 0)
            return new Vector2(totalWidth, totalHeight);

        return new Vector2(Math.Min(totalWidth, availW), Math.Min(totalHeight, availH));
    }

    protected internal override float GetHeight(float maxWidth)
        => GetSize(new Rectangle(0, 0, (int)maxWidth, int.MaxValue)).Y;

    // Drawing
    protected override void Draw(Rectangle bounds)
    {
        UpdateFontSize(bounds.Width);

        optionColumns = new List<List<Rectangle>>();

        if (options.Count == 0) return;

        // subtle group card (thin border, no heavy fill)
        var groupRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        ray.DrawRectangleRec(groupRect, new Color(0, 0, 0, 0)); // transparent
        ray.DrawRectangleLinesEx(groupRect, 1f, Style.RadioGroupBorder);

        float availW = bounds.Width;
        float availH = bounds.Height;

        UpdateFontSize(availW);

        // measure
        List<Rectangle> measured = new();
        for (int i = 0; i < options.Count; i++)
            measured.Add(MeasureOption(options[i], availW));

        // pack into columns
        List<float> colWidths = new();
        List<float> colHeights = new();
        List<List<Rectangle>> cols = new();

        float curColHeight = 0f;
        float curColWidth = 0f;
        List<Rectangle> curCol = new();

        for (int i = 0; i < measured.Count; i++)
        {
            var size = measured[i];
            float needed = (curCol.Count == 0) ? size.Height : (SPACING + size.Height);

            if (curCol.Count > 0 && curColHeight + needed > availH && availH > 0)
            {
                cols.Add(curCol);
                colWidths.Add(curColWidth);
                colHeights.Add(curColHeight);

                curCol = new List<Rectangle>();
                curColHeight = 0f;
                curColWidth = 0f;
            }

            curCol.Add(size);
            curColHeight = (curCol.Count == 1) ? size.Height : curColHeight + SPACING + size.Height;
            curColWidth = Math.Max(curColWidth, size.Width);
        }

        if (curCol.Count > 0)
        {
            cols.Add(curCol);
            colWidths.Add(curColWidth);
            colHeights.Add(curColHeight);
        }

        // Position columns left-to-right, each column's items top-to-bottom
        float colX = bounds.X;
        for (int c = 0; c < cols.Count; c++)
        {
            var col = cols[c];
            float colW = colWidths[c];
            float colY = bounds.Y;

            optionColumns.Add(new List<Rectangle>());

            for (int i = 0; i < col.Count; i++)
            {
                var size = col[i];
                var positioned = new Rectangle((int)colX, (int)colY, (int)size.Width, (int)size.Height);
                optionColumns[c].Add(positioned);
                colY += size.Height + SPACING;
            }

            colX += colW + SPACING;
        }

        // Now draw each option using the unified style
        for (int c = 0; c < optionColumns.Count; c++)
        {
            var col = optionColumns[c];
            for (int i = 0; i < col.Count; i++)
            {
                var rect = col[i];
                int flatIndex = optionColumns.Take(c).Sum(cc => cc.Count) + i;

                if (flatIndex < 0 || flatIndex >= options.Count) continue;
                var opt = options[flatIndex];

                // subtle per-row hover highlight
                if (opt.IsHovered)
                {
                    var hoverRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                    ray.DrawRectangleRec(hoverRect, Style.ButtonHover);
                }

                // draw radio dot (circle)
                float dotX = rect.X + PADDING_X;
                float dotY = rect.Y + (rect.Height / 2f);

                int outerRadius = DOT_SIZE / 2;
                // outer circle outline
                Raylib_cs.Raylib.DrawCircleLines((int)(dotX + outerRadius), (int)dotY, outerRadius, Style.ButtonBorder);

                // inner fill radius scales with animation progress
                float innerMaxRadius = outerRadius - 3f; // inset so outline visible
                float fillRadius = innerMaxRadius * opt.AnimationProgress;
                if (fillRadius > 0.1f)
                {
                    byte alpha = (byte)(255 * Math.Clamp(opt.AnimationProgress, 0f, 1f));
                    Color bb = Style.ButtonBorder;
                    var fillColor = new Color(bb.R, bb.G, bb.B, alpha);
                    Raylib_cs.Raylib.DrawCircle((int)(dotX + outerRadius), (int)dotY, fillRadius, fillColor);
                }

                // Draw label text
                float textX = dotX + DOT_SIZE + SPACING;
                float textY = rect.Y + (rect.Height - labelFontSize) / 2f;
                opt.Text.FontSize = labelFontSize;
                RichTextRenderer.DrawRichText(
                    opt.Text,
                    new Vector2(textX, textY),
                    rect.Width - (textX - rect.X),
                    new Color(255, 255, 255, (int)(255 * Style.ContentAlpha)),
                    Input);

                // If selected, draw a subtle accent line or glow to indicate the group relation
                if (opt.Selected)
                {
                    // thin accent line on the left of the row to indicate current selection in the group
                    float accentX = rect.X;
                    float accentY1 = rect.Y + 2;
                    float accentY2 = rect.Y + rect.Height - 2;
                    ray.DrawLine((int)accentX, (int)accentY1, (int)accentX, (int)accentY2, Style.RadioGroupAccent);
                }
            }
        }
    }
}
