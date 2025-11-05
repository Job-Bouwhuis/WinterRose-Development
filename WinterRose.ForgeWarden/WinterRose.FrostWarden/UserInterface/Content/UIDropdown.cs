using Raylib_cs;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.Utility;

namespace WinterRose.ForgeWarden.UserInterface;

public class UIDropdown<T> : UIContent
{
    public MulticastVoidInvocation<UIDropdown<T>, List<T>> OnSelected { get; set; } = new();

    private readonly UITextInput filterInput;
    private readonly List<T> items = new();
    private readonly List<int> filteredIndices = new();
    private readonly Func<T, string> stringSelector;

    // configurable
    public bool AllowTyping { get; set; } = true;
    public bool AllowCustomInput { get; set; } = false;
    public int MaxVisibleItems { get; set; } = 6;
    public float ItemPadding = 6f;
    public float ItemSpacing = 2f;

    // state
    private bool isOpen = false;
    private int selectedIndex = -1;         // index into items for currently selected item
    private int highlightedIndex = 0;       // index into filteredIndices for keyboard hover
    private Rectangle lastBounds;

    public IReadOnlyList<T> Items => items;
    public int SelectedIndex
    {
        get => selectedIndex;
        set => selectedIndex = value;
    }
    public T? SelectedItem =>
        !MultiSelect && selectedIndex >= 0 && selectedIndex < items.Count
            ? items[selectedIndex]
            : default;

    public bool MultiSelect { get; set; } = false;
    private readonly HashSet<int> selectedIndices = new();

    // Adjusted selected items accessor
    public IReadOnlyList<T> SelectedItems => selectedIndices.Select(i => items[i]).ToList();



    private float scrollOffset = 0f;
    private bool isDraggingScrollbar = false;
    private float dragStartY = 0f;
    private float dragStartOffset = 0f;

    InputContext dropdownInput;

    public UIDropdown(Func<T, string> stringSelector)
    {
        filterInput = new();
        // default placeholder on filter
        filterInput.OnInputChanged.Subscribe(Invocation.Create<UITextInput, string>((_, __) => Refilter()));
        filterInput.OnSubmit.Subscribe(Invocation.Create<UITextInput, string>((_, text) =>
        {
            // on enter in filter, pick highlighted if any
            if (isOpen)
            {
                if (filteredIndices.Count > 0)
                {
                    CommitSelection(filteredIndices[Math.Clamp(highlightedIndex, 0, filteredIndices.Count - 1)]);
                }
                else if (AllowCustomInput && !string.IsNullOrWhiteSpace(text))
                {
                    CommitCustomInput(text);
                }
            }
        }));
        this.stringSelector = stringSelector;
    }

    public UIDropdown() : this(o => o.ToString()) { }

    public void SetItems(IEnumerable<T> newItems)
    {
        items.Clear();
        items.AddRange(newItems ?? Array.Empty<T>());
        Refilter();
    }

    private void Refilter()
    {
        filteredIndices.Clear();
        string q = filterInput.Text ?? "";
        if (string.IsNullOrWhiteSpace(q))
        {
            for (int i = 0; i < items.Count; i++)
                filteredIndices.Add(i);
        }
        else
        {
            var found = items.SearchMany(q, stringSelector, Fuzzy.ComparisonType.IgnoreCase);
            foreach (var f in found)
            {
                if (f.score > 0.1)
                    filteredIndices.Add(items.IndexOf(f.item));
            }
        }

        // reset keyboard highlight
        highlightedIndex = 0;
    }

    protected internal override void Setup()
    {
        filterInput.owner = owner;
        dropdownInput = new(new RaylibInputProvider(), Input.Priority + 1);
        base.Setup();
        filterInput.Setup();
        Refilter();
    }

    protected internal override void Update()
    {
        base.Update();

        // Allow typing when open
        if (AllowTyping && isOpen)
            filterInput.UpdateInline();

        if (isOpen)
        {
            // keyboard navigation
            if (Input.IsPressed(KeyboardKey.Down))
                highlightedIndex = Math.Min(filteredIndices.Count - 1, highlightedIndex + 1);

            if (Input.IsPressed(KeyboardKey.Up))
                highlightedIndex = Math.Max(0, highlightedIndex - 1);

            if (Input.IsPressed(KeyboardKey.Enter))
            {
                if (filteredIndices.Count > 0)
                {
                    int idx = filteredIndices[Math.Clamp(highlightedIndex, 0, filteredIndices.Count - 1)];

                    if (MultiSelect)
                    {
                    }
                    else
                    {
                        CommitSelection(idx);
                    }
                }
            }

            if (Input.IsPressed(KeyboardKey.Escape))
                CloseDropdown();
        }
        else
        {
            InputManager.UnregisterContext(dropdownInput);
        }
    }


    private void CommitSelection(int itemIdx)
    {
        if (MultiSelect)
        {
            // Toggle selection in multi-select mode
            if (selectedIndices.Contains(itemIdx))
                selectedIndices.Remove(itemIdx);
            else
                selectedIndices.Add(itemIdx);

            // Fire event with all currently selected items
            OnSelected?.Invoke(this, SelectedItems.ToList());
        }
        else
        {
            selectedIndex = itemIdx;
            OnSelected?.Invoke(this, new List<T>() { items[itemIdx] });
            CloseDropdown();
        }

        UpdateScrollOffset(itemIdx);
    }

    private void UpdateScrollOffset(int itemIdx)
    {
        float itemHeight = MathF.Max(Style.TextBoxMinHeight, Style.TextBoxFontSize + Style.TextBoxTextSpacing * 2f);
        int visibleCount = Math.Min(MaxVisibleItems, filteredIndices.Count);
        float listHeight = itemHeight * visibleCount;
        float totalHeight = filteredIndices.Count * itemHeight;

        if (totalHeight > listHeight)
        {
            float targetOffset = filteredIndices.IndexOf(itemIdx) * itemHeight - listHeight / 2 + itemHeight / 2;
            scrollOffset = Math.Clamp(targetOffset, 0, totalHeight - listHeight);
        }
        else
        {
            scrollOffset = 0f;
        }
    }

    private void CommitCustomInput(string text)
    {
        OnSelected?.Invoke(this, new List<string>() { text });
    }

    private void OpenDropdown()
    {
        isOpen = true;
        Refilter();
        InputManager.RegisterContext(dropdownInput);
        dropdownInput.HasMouseFocus = false;
        if (AllowTyping)
        {
            filterInput.Focus();
        }
    }

    private void CloseDropdown()
    {
        isOpen = false;
        filterInput.Blur();

        InputManager.UnregisterContext(dropdownInput);
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        // width uses available width; height is the line height of the text box
        float h = filterInput.MeasureHeight(availableArea.Width);
        return new Vector2(availableArea.Width, h);
    }

    protected override void Draw(Rectangle bounds)
    {
        lastBounds = bounds;

        Rectangle box = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        ray.DrawRectangleRec(box, Style.TextBoxBackground);
        ray.DrawRectangleLinesEx(box, 1f, Style.TextBoxBorder);

        var font = Raylib.GetFontDefault();
        float fontSize = Style.TextBoxFontSize;
        float spacing = Style.TextBoxTextSpacing;
        Vector2 textPos = new Vector2(box.X + Style.TextBoxTextSpacing, box.Y + (box.Height - fontSize) / 2f - 1f);

        string selText;
        bool nothingSelectedForDisplay;
        if (MultiSelect)
        {
            if (selectedIndices.Count > 0)
            {
                var ordered = selectedIndices.OrderBy(i => i).Select(i => stringSelector(items[i]) ?? "");
                selText = string.Join(", ", ordered);
                nothingSelectedForDisplay = false;
            }
            else
            {
                selText = AllowTyping ? filterInput.Placeholder : "";
                nothingSelectedForDisplay = true;
            }
        }
        else
        {
            selText = SelectedItem is not null ? stringSelector(SelectedItem) : (AllowTyping ? filterInput.Placeholder : "");
            nothingSelectedForDisplay = SelectedItem is null;
        }

        Color drawColor = nothingSelectedForDisplay
            ? new Color(160, 160, 160).WithAlpha(Style.ContentAlpha)
            : Style.TextBoxText;

        Raylib.DrawTextEx(font, selText ?? "", textPos, fontSize, spacing, drawColor);

        if (isOpen)
        {
            Application.Current.AddDebugDraw(() =>
            {
                float itemHeight = MathF.Max(Style.TextBoxMinHeight, Style.TextBoxFontSize + Style.TextBoxTextSpacing * 2f);
                float totalHeight = filteredIndices.Count * itemHeight;
                int visibleCount = Math.Min(MaxVisibleItems, Math.Max(0, filteredIndices.Count));
                float listHeight = itemHeight * visibleCount;

                Rectangle listRect = new Rectangle(box.X, box.Y + box.Height, box.Width, listHeight);
                dropdownInput.IsRequestingMouseFocus =
                    ray.CheckCollisionPointRec(Input.Provider.MousePosition, listRect);

                if (totalHeight > listHeight && Raylib.CheckCollisionPointRec(Input.Provider.MousePosition, listRect))
                {
                    float wheel = ray.GetMouseWheelMove();
                    if (wheel != 0)
                    {
                        scrollOffset -= wheel * itemHeight;
                        scrollOffset = Math.Clamp(scrollOffset, 0, Math.Max(0, totalHeight - listHeight));
                    }
                }

                ray.DrawRectangleRec(listRect, Style.TextBoxBackground);
                ray.DrawRectangleLinesEx(listRect, 1f, Style.TextBoxBorder);

                ScissorStack.Push(listRect);

                for (int i = 0; i < filteredIndices.Count; i++)
                {
                    float y = listRect.Y + i * itemHeight - scrollOffset;
                    if (y + itemHeight < listRect.Y || y > listRect.Y + listRect.Height)
                        continue;

                    int itemIndex = filteredIndices[i];
                    Rectangle itemRect = new Rectangle(listRect.X, y, listRect.Width, itemHeight);

                    bool mouseOver = Raylib.CheckCollisionPointRec(Input.Provider.MousePosition, itemRect);
                    bool highlighted = (i == highlightedIndex);

                    bool isSelected = MultiSelect
                        ? selectedIndices.Contains(itemIndex)
                        : (itemIndex == selectedIndex);

                    if (mouseOver)
                        ray.DrawRectangleRec(itemRect, Style.ScrollbarThumb);
                    else if (highlighted)
                        ray.DrawRectangleRec(itemRect, new Color(100, 100, 100, 120));

                    // start text position; may be moved right if there's a checkbox
                    float itPosX = itemRect.X + ItemPadding;
                    float itPosY = itemRect.Y + (itemRect.Height - fontSize) / 2f - 1f;

                    // draw multi-select checkbox first (so text never overlaps it)
                    if (MultiSelect)
                    {
                        float boxSize = itemHeight * 0.5f;
                        float boxX = itemRect.X + ItemPadding;
                        float boxY = itemRect.Y + (itemHeight - boxSize) / 2f;
                        var checkRect = new Rectangle(boxX, boxY, boxSize, boxSize);

                        ray.DrawRectangleLinesEx(checkRect, 1f, Style.TextBoxText);

                        if (isSelected)
                            ray.DrawRectangleRec(checkRect, Style.TextBoxText);

                        // move text start to the right of the checkbox
                        itPosX = boxX + boxSize + ItemSpacing;
                    }

                    // draw the item text at adjusted position
                    Vector2 itPos = new Vector2(itPosX, itPosY);
                    Raylib.DrawTextEx(font, stringSelector(items[itemIndex]) ?? "", itPos, fontSize, spacing, Style.TextBoxText);

                    // mouse click handling remains the same (no change)
                    if (mouseOver && dropdownInput.Provider.IsPressed(new InputBinding(InputDeviceType.Mouse, 0)))
                    {
                        if (itemRect.X >= listRect.X &&
                            itemRect.Y >= listRect.Y &&
                            itemRect.X + itemRect.Width <= listRect.X + listRect.Width &&
                            itemRect.Y + itemRect.Height <= listRect.Y + listRect.Height)
                        {
                            CommitSelection(itemIndex);
                            Input.IsRequestingMouseFocus = true;
                            ScissorStack.Pop();
                            return;
                        }
                    }
                }

                ScissorStack.Pop();

                if (totalHeight > listHeight)
                {
                    float scrollbarHeight = listHeight * (listHeight / totalHeight);
                    float scrollbarY = listRect.Y + (scrollOffset / (totalHeight - listHeight)) * (listHeight - scrollbarHeight);

                    Rectangle scrollbarRect = new Rectangle(listRect.X + listRect.Width - 6, scrollbarY, 6, scrollbarHeight);
                    ray.DrawRectangleRec(scrollbarRect, Style.ScrollbarThumb);
                }

                if (AllowTyping)
                {
                    Rectangle inputRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                    filterInput.RenderInline(inputRect);
                }

                float arrowSize = box.Height * 0.5f;
                var arrowCenter = new Vector2(box.X + box.Width - Style.TextBoxTextSpacing - arrowSize, box.Y + box.Height / 2f);

                Vector2 a = new(arrowCenter.X - arrowSize * 0.5f, arrowCenter.Y - arrowSize * 0.25f);
                Vector2 b = new(arrowCenter.X + arrowSize * 0.5f, arrowCenter.Y - arrowSize * 0.25f);
                Vector2 c = new(arrowCenter.X, arrowCenter.Y + arrowSize * 0.35f);
                ray.DrawTriangle(a, b, c, Style.TextBoxText);
            });
        }
    }


    protected internal override float GetHeight(float maxWidth)
    {
        // standard height is the text box height
        return filterInput.MeasureHeight(maxWidth);
    }

    protected internal override void OnContentClicked(MouseButton button)
    {
        if (button == MouseButton.Left)
        {
            // toggle when clicking the closed box
            if (!isOpen)
            {
                OpenDropdown();
            }
            else
            {
                CloseDropdown();
            }
        }
    }

    protected internal override void OnClickedOutsideOfContent(MouseButton button)
    {
        // clicking outside should close the dropdown
        CloseDropdown();
    }
    protected internal override void OnOwnerClosing()
    {
        CloseDropdown();
    }

    public void AddOption(T option) => items.Add(option);
}
