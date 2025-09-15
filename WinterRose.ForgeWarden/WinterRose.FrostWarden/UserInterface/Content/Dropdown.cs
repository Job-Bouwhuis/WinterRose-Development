using Raylib_cs;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.Input;

namespace WinterRose.ForgeWarden.UserInterface.Content;

public class Dropdown<T> : UIContent
{
    public MulticastVoidInvocation<Dropdown<T>, T> OnSelected = new();

    private readonly TextInput filterInput;
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
    public int SelectedIndex => selectedIndex;
    public T SelectedItem => selectedIndex >= 0 && selectedIndex < items.Count ? items[selectedIndex] : default;

    private float scrollOffset = 0f;
    private bool isDraggingScrollbar = false;
    private float dragStartY = 0f;
    private float dragStartOffset = 0f;

    InputContext dropdownInput;

    public Dropdown(Func<T, string> stringSelector)
    {
        filterInput = new();
        // default placeholder on filter
        filterInput.OnInputChanged.Subscribe(Invocation.Create<TextInput, string>((_, __) => Refilter()));
        filterInput.OnSubmit.Subscribe(Invocation.Create<TextInput, string>((_, text) =>
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

    public Dropdown() : this(o => o.ToString()) { }

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
                if (f.score > 0)
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

        // If typing is enabled and the dropdown is open we let the filterInput consume keyboard via UpdateInline
        if (AllowTyping && isOpen)
        {
            filterInput.UpdateInline();
        }

        // Keyboard navigation when open and not typing (or even when typing we allow arrows)
        if (isOpen)
        {
            if (Input.IsPressed(KeyboardKey.Down))
            {
                highlightedIndex = Math.Min(filteredIndices.Count - 1, highlightedIndex + 1);
            }
            if (Input.IsPressed(KeyboardKey.Up))
            {
                highlightedIndex = Math.Max(0, highlightedIndex - 1);
            }
            if (Input.IsPressed(KeyboardKey.Enter))
            {
                if (filteredIndices.Count > 0)
                {
                    CommitSelection(filteredIndices[Math.Clamp(highlightedIndex, 0, filteredIndices.Count - 1)]);
                }
            }
            if (Input.IsPressed(KeyboardKey.Escape))
            {
                CloseDropdown();
            }
        }
        else
        {
            InputManager.UnregisterContext(dropdownInput);
        }
    }

    private void CommitSelection(int itemIdx)
    {
        selectedIndex = itemIdx;
        OnSelected?.Invoke(this, items[itemIdx]);

        float itemHeight = MathF.Max(Style.TextBoxMinHeight, Style.TextBoxFontSize + Style.TextBoxTextSpacing * 2f);
        int visibleCount = Math.Min(MaxVisibleItems, filteredIndices.Count);
        float listHeight = itemHeight * visibleCount;
        float totalHeight = filteredIndices.Count * itemHeight;

        if (totalHeight > listHeight)
        {
            // Make selected item appear in the middle of the visible list
            float targetOffset = filteredIndices.IndexOf(itemIdx) * itemHeight - listHeight / 2 + itemHeight / 2;

            // Clamp to valid scroll range
            scrollOffset = Math.Clamp(targetOffset, 0, totalHeight - listHeight);
        }
        else
        {
            scrollOffset = 0f;
        }

        CloseDropdown();
    }



    private void CommitCustomInput(string text)
    {
        OnSelected?.Invoke(this, text);
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

        string selText = SelectedItem is not null ? stringSelector(SelectedItem) : (AllowTyping ? filterInput.Placeholder : "");
        Color drawColor = SelectedItem == null ? new Color(160, 160, 160, (int)(255 * Style.ContentAlpha)) : Style.TextBoxText;
        Raylib.DrawTextEx(font, selText ?? "", textPos, fontSize, spacing, drawColor);

        if (isOpen)
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

            Raylib.BeginScissorMode((int)listRect.X, (int)listRect.Y, (int)listRect.Width, (int)listRect.Height);

            for (int i = 0; i < filteredIndices.Count; i++)
            {
                float y = listRect.Y + i * itemHeight - scrollOffset;
                if (y + itemHeight < listRect.Y || y > listRect.Y + listRect.Height)
                    continue;

                int itemIndex = filteredIndices[i];
                Rectangle itemRect = new Rectangle(listRect.X, y, listRect.Width, itemHeight);

                bool mouseOver = Raylib.CheckCollisionPointRec(Input.Provider.MousePosition, itemRect);
                bool highlighted = (i == highlightedIndex);

                if (mouseOver)
                    ray.DrawRectangleRec(itemRect, Style.ScrollbarThumb);
                else if (highlighted)
                    ray.DrawRectangleRec(itemRect, new Color(100, 100, 100, 120));

                Vector2 itPos = new Vector2(itemRect.X + ItemPadding, itemRect.Y + (itemRect.Height - fontSize) / 2f - 1f);
                Raylib.DrawTextEx(font, stringSelector(items[itemIndex]) ?? "", itPos, fontSize, spacing, Style.TextBoxText);

                if (mouseOver && dropdownInput.Provider.IsPressed(new InputBinding(InputDeviceType.Mouse, 0)))
                {
                    if (itemRect.X >= listRect.X &&
                        itemRect.Y >= listRect.Y &&
                        itemRect.X + itemRect.Width <= listRect.X + listRect.Width &&
                        itemRect.Y + itemRect.Height <= listRect.Y + listRect.Height)
                    {
                        CommitSelection(itemIndex);
                        Input.IsRequestingMouseFocus = true;
                        Raylib.EndScissorMode();
                        return;
                    }
                }
            }

            Raylib.EndScissorMode();

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
}
