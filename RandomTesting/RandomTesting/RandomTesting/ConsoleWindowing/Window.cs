namespace RandomTesting.ConsoleWindowing
{
    // ========== WINDOW ==========
    public class Window
    {
        public string Title { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int PreferredWidth { get; set; }
        public int PreferredHeight { get; set; }
        public ConsoleColor BorderColor { get; set; } = ConsoleColor.White;
        public ConsoleColor TitleColor { get; set; } = ConsoleColor.Cyan;
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
        public bool IsFocused { get; set; }
        public bool IsDirty { get; private set; } = true;
        private bool _needsFullRedraw = true;
        public int ZIndex { get; set; } = 1;

        private int _scrollOffset = 0;
        public int ScrollOffset
        {
            get => _scrollOffset;
            set
            {
                int maxOffset = Math.Max(0, TotalContentHeight - MaxVisibleLines);
                _scrollOffset = Math.Clamp(value, 0, maxOffset);
                MarkDirty();
            }
        }

        public int MaxVisibleLines => Math.Max(0, Height - 2);
        public int TotalContentHeight
        {
            get
            {
                if (Root == null) return 0;
                return Root.Height;
            }
        }

        public LayerStack Root { get; private set; }

        public Window(string title, int x, int y, int width, int height)
        {
            Title = title;
            X = x; Y = y;
            PreferredWidth = width;
            PreferredHeight = height;
            Width = width;
            Height = height;
            Root = new LayerStack(Orientation.Vertical);
        }

        public void OnFocus() => IsFocused = true;

        public void SetRoot(LayerStack root)
        {
            Root = root;
            Root.OnWindowSet(this);
            MarkFullRedraw();
        }

        public void AddControl(Control ctrl)
        {
            Root.AddChild(ctrl);
            ctrl.Window = this;
            ctrl.OnWindowSet(this);
            MarkFullRedraw();
        }

        public void RemoveControl(Control ctrl)
        {
            if (Root.Children.Contains(ctrl))
            {
                Root.Children.Remove(ctrl);
                ctrl.OnWindowUnset();
                MarkDirty();
            }
        }

        public void ScrollUp(int lines) => ScrollOffset -= lines;
        public void ScrollDown(int lines) => ScrollOffset += lines;
        public void ScrollPageUp() => ScrollOffset -= MaxVisibleLines;
        public void ScrollPageDown() => ScrollOffset += MaxVisibleLines;

        public void ScrollToControl(Control ctrl)
        {
            if (ctrl == null) return;
            var (_, y) = ctrl.GetAbsolutePosition();
            int top = y;
            int bottom = y + ctrl.Height - 1;
            int visibleTop = ScrollOffset + 1;
            int visibleBottom = ScrollOffset + MaxVisibleLines;

            if (top < visibleTop)
                ScrollOffset = top - 1;
            else if (bottom > visibleBottom)
                ScrollOffset = bottom - MaxVisibleLines;
        }

        private (int top, int bottom) GetControlY(Control ctrl)
        {
            var (_, y) = ctrl.GetAbsolutePosition();
            return (y, y + ctrl.Height - 1);
        }

        private LayerStack GetParentStack(LayoutElement elem)
        {
            return FindParent(Root, elem);
        }

        private LayerStack FindParent(LayerStack stack, LayoutElement target)
        {
            if (stack.Children.Contains(target))
                return stack;
            foreach (var child in stack.Children)
            {
                if (child is LayerStack childStack)
                {
                    var result = FindParent(childStack, target);
                    if (result != null) return result;
                }
            }
            return null;
        }

        public void ClampScrollOffset()
        {
            int maxOffset = Math.Max(0, TotalContentHeight - MaxVisibleLines);
            if (_scrollOffset > maxOffset)
                _scrollOffset = maxOffset;
            MarkDirty();
        }

        public void MarkDirty() => IsDirty = true;
        public void MarkFullRedraw()
        {
            _needsFullRedraw = true;
            MarkDirty();
        }

        public void AdjustSize(int consoleWidth, int consoleHeight)
        {
            int availableWidth = Math.Max(2, consoleWidth - 1);
            int availableHeight = Math.Max(2, consoleHeight - 1);
            int newWidth = Math.Clamp(PreferredWidth, 2, availableWidth);
            int newHeight = Math.Clamp(PreferredHeight, 2, availableHeight);
            if (X + newWidth > consoleWidth) X = consoleWidth - newWidth - 1;
            if (Y + newHeight > consoleHeight) Y = consoleHeight - newHeight - 1;
            if (X < 0) X = 0;
            if (Y < 0) Y = 0;
            bool sizeChanged = Width != newWidth || Height != newHeight;
            Width = newWidth;
            Height = newHeight;
            if (sizeChanged)
                MarkFullRedraw();
            ClampScrollOffset();
        }

        public void Draw()
        {
            if (!IsDirty) return;

            ConsoleColor borderColor = BorderColor;
            if (Screen.IsWindowSelected(GetWindowIndex()))
            {
                if (Screen.IsBorderBlinkVisible())
                {
                    if (borderColor == ConsoleColor.White)
                        borderColor = ConsoleColor.Gray;
                    else
                        borderColor = ConsoleColor.White;
                }
            }

            if (_needsFullRedraw)
            {
                Renderer.ClearArea(X + 1, Y + 1, Width - 2, Height - 2, BackgroundColor);
                Renderer.DrawBox(X, Y, Width, Height, GetDisplayTitle(), borderColor, TitleColor, BackgroundColor);
                _needsFullRedraw = false;
            }
            else
            {
                Renderer.DrawBox(X, Y, Width, Height, GetDisplayTitle(), borderColor, TitleColor, BackgroundColor);
            }

            int contentWidth = Width - 2;
            int contentHeight = Height - 2;
            if (contentWidth <= 0 || contentHeight <= 0)
            {
                IsDirty = false;
                return;
            }

            Root.Layout(contentWidth, contentHeight);

            Root.Draw(X + 1, Y + 1 - ScrollOffset);

            IsDirty = false;
        }

        public IEnumerable<Control> GetAllFocusableControls()
        {
            return Root.GetFocusableControls();
        }

        private int GetWindowIndex()
        {
            for (int i = 0; i < Screen.windows.Count; i++)
                if (Screen.windows[i] == this) return i;
            return -1;
        }

        private string GetDisplayTitle()
        {
            if (Width >= PreferredWidth && Height >= PreferredHeight)
                return Title;
            return $"{Title} - smaller than preferred";
        }

        public void HandleShutdownKey(ConsoleKeyInfo key) => Screen.HandleShutdownKey(this, key);
        public void Close() => Screen.RemoveWindow(this);
        internal void Open() => Screen.AddWindow(this);
    }
}
