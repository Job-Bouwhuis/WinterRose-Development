using WinterRose.FuzzySearching;

namespace RandomTesting.WebsitePreviewFetcher
{
    // ========== TEXT INPUT ==========
    public class TextInput : Control
    {
        public event Action<string> TextChanged;
        private int _cursorPos;

        public TextInput(string initialText, int width)
        {
            Text = initialText;
            Width = width;
            Height = 1;
            _cursorPos = Text.Length;
            Focusable = true;
            IsActive = false;
        }

        public override bool IsActive { get; protected set; }
        public override bool ConsumesNavigation => IsActive;

        public override void Layout(int maxWidth, int maxHeight)
        {
            // fixed width/height
        }

        public override void Draw(int offsetX, int offsetY)
        {
            int drawX = offsetX + X;
            int drawY = offsetY + Y;
            ConsoleColor bg = IsActive ? ConsoleColor.DarkBlue : (IsFocused ? ConsoleColor.DarkGray : BackColor);
            Renderer.DrawString(drawX, drawY, new string(' ', Width), null, bg);

            string display = Text.PadRight(Width);
            if (display.Length > Width) display = display[..Width];
            Renderer.DrawString(drawX, drawY, display, ForeColor, bg);

            if (IsActive)
            {
                int cursorPos = Math.Min(_cursorPos, Width - 1);
                int cursorX = drawX + cursorPos;
                char ch = (cursorPos < Text.Length) ? Text[cursorPos] : ' ';
                if (Screen.IsCursorVisible())
                    Renderer.DrawChar(cursorX, drawY, '_', ConsoleColor.White, bg);
                else
                    Renderer.DrawChar(cursorX, drawY, ch, ForeColor, bg);
            }
        }

        public override void HandleKey(ConsoleKeyInfo key)
        {
            if (!Enabled || !IsActive) return;

            if (key.Key == ConsoleKey.Escape)
            {
                Cancel();
                MarkDirty();
                return;
            }

            if (key.Key == ConsoleKey.LeftArrow)
            {
                if (_cursorPos > 0) _cursorPos--;
                MarkDirty();
                return;
            }
            if (key.Key == ConsoleKey.RightArrow)
            {
                if (_cursorPos < Text.Length) _cursorPos++;
                MarkDirty();
                return;
            }
            if (key.Key == ConsoleKey.UpArrow)
            {
                _cursorPos = 0;
                MarkDirty();
                return;
            }
            if (key.Key == ConsoleKey.DownArrow)
            {
                _cursorPos = Text.Length;
                MarkDirty();
                return;
            }

            if (key.Key == ConsoleKey.Backspace && _cursorPos > 0)
            {
                Text = Text.Remove(_cursorPos - 1, 1);
                _cursorPos--;
                TextChanged?.Invoke(Text);
                MarkDirty();
                return;
            }
            if (key.Key == ConsoleKey.Delete && _cursorPos < Text.Length)
            {
                Text = Text.Remove(_cursorPos, 1);
                TextChanged?.Invoke(Text);
                MarkDirty();
                return;
            }
            if (key.Key == ConsoleKey.Home)
            {
                _cursorPos = 0;
                MarkDirty();
                return;
            }
            if (key.Key == ConsoleKey.End)
            {
                _cursorPos = Text.Length;
                MarkDirty();
                return;
            }

            if (key.KeyChar >= 32 && key.KeyChar < 127)
            {
                Text = Text.Insert(_cursorPos, key.KeyChar.ToString());
                _cursorPos++;
                TextChanged?.Invoke(Text);
                MarkDirty();
            }
        }

        public override void Activate()
        {
            if (!Enabled) return;
            if (!IsActive)
            {
                IsActive = true;
                MarkDirty();
            }
            else
            {
                IsActive = false;
                TextChanged?.Invoke(Text);
                MarkDirty();
            }
        }

        public override void Cancel()
        {
            if (IsActive)
            {
                IsActive = false;
                MarkDirty();
            }
        }

        public override void OnBlur()
        {
            if (IsActive)
            {
                IsActive = false;
                MarkDirty();
            }
            base.OnBlur();
        }
    }
}