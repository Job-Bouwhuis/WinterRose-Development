using System;
using System.Collections.Generic;

namespace RandomTesting.WebsitePreviewFetcher
{
    public class TreeNode : Control
    {
        private List<Control> _children = new List<Control>();
        public IList<Control> Children => _children;

        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    UpdateVisibility();
                    MarkDirty();
                }
            }
        }

        private LayerStack _childStack;

        public TreeNode(string text, int width)
        {
            Text = text;
            Width = width;
            Height = 1;
            Focusable = true;
            _childStack = new LayerStack(Orientation.Vertical)
            {
                IsVisible = false,
                Parent = this  // crucial: link child stack to the node
            };
        }

        public void AddChild(Control child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (_children.Contains(child)) return;
            _children.Add(child);
            _childStack.AddChild(child); // sets child.Parent = _childStack
            child.Window = this.Window;
            UpdateVisibility();
        }

        public void RemoveChild(Control child)
        {
            if (_children.Remove(child))
            {
                _childStack.Children.Remove(child);
                child.Window = null;
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            _childStack.IsVisible = IsExpanded;
            if (IsExpanded)
            {
                foreach (var c in _children)
                    c.Window = this.Window;
            }
            Window?.MarkFullRedraw();
        }

        public override void Layout(int maxWidth, int maxHeight)
        {
            if (_childStack.IsVisible)
            {
                _childStack.Layout(maxWidth, maxHeight - 1);
                _childStack.X = 0;
                _childStack.Y = 1; // below the node
                Height = 1 + _childStack.Height;
            }
            else
            {
                Height = 1;
                _childStack.Width = 0;
                _childStack.Height = 0;
            }
        }

        public override void Draw(int offsetX, int offsetY)
        {
            int drawX = offsetX + X;
            int drawY = offsetY + Y;

            string indent = Text[..^Text.TrimStart().Length];
            string label = Text.TrimStart();
            string indicator = IsExpanded ? "[-]" : "[+]";
            string fullText = $"{indent}{indicator} {label}";
            string display = fullText.PadRight(Width);
            if (display.Length > Width) display = display[..Width];

            ConsoleColor bg = IsFocused ? ConsoleColor.DarkGray : BackColor;
            Renderer.DrawString(drawX, drawY, new string(' ', Width), null, bg);
            Renderer.DrawString(drawX, drawY, display, ForeColor, bg);

            if (_childStack.IsVisible)
            {
                _childStack.X = 0;
                _childStack.Y = 1;
                _childStack.Draw(offsetX + X, offsetY + Y);
            }
        }

        public override IEnumerable<Control> GetFocusableControls()
        {
            if (Focusable && Enabled)
                yield return this;
            if (IsExpanded)
                foreach (var c in _childStack.GetFocusableControls())
                    yield return c;
        }

        public override void Activate()
        {
            if (!Enabled) return;
            IsExpanded = !IsExpanded;
            MarkDirty();
        }

        public override void Cancel() { }

        public override void OnWindowSet(Window window)
        {
            base.OnWindowSet(window);
            _childStack.OnWindowSet(window);
            foreach (var c in _children)
                c.Window = window;
        }

        public override void OnWindowUnset()
        {
            _childStack.OnWindowUnset();
            foreach (var c in _children)
                c.Window = null;
            base.OnWindowUnset();
        }
    }
}
