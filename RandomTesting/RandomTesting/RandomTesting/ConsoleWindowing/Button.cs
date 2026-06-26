namespace RandomTesting.ConsoleWindowing
{
    // ========== BUTTON ==========
    public class Button : Control
    {
        public event Action Click;
        private bool _isPressed;

        public Button(string text)
        {
            Text = text;
            Width = text.Length + 4;
            Height = 1;
            Focusable = true;
        }

        public override void Layout(int maxWidth, int maxHeight)
        {
            // fixed size
        }

        public override void Draw(int offsetX, int offsetY)
        {
            int drawX = offsetX + X;
            int drawY = offsetY + Y;
            ConsoleColor fg = _isPressed ? BackColor : (IsFocused ? ConsoleColor.Black : ForeColor);
            ConsoleColor bg = _isPressed ? ForeColor : (IsFocused ? ConsoleColor.White : BackColor);
            Renderer.DrawString(drawX, drawY, new string(' ', Width), null, bg);
            Renderer.DrawString(drawX, drawY, $"[{Text}]", fg, bg);
        }

        public override void Activate()
        {
            if (!Enabled) return;
            _isPressed = true;
            Click?.Invoke();
            _isPressed = false;
            MarkDirty();
        }

        public override void HandleKey(ConsoleKeyInfo key)
        {
            if (!Enabled) return;
            if (key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Spacebar)
                Activate();
        }
    }
}