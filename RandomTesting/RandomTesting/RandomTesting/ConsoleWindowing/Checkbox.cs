namespace RandomTesting.ConsoleWindowing
{
    public class Checkbox : Control
    {
        public event Action<bool> CheckedChanged;
        public bool IsChecked { get; private set; }

        public Checkbox(string text, bool isChecked = false)
        {
            Text = text;
            IsChecked = isChecked;
            Width = text.Length + 4;
            Height = 1;
            Focusable = true;
        }

        public override void Layout(int maxWidth, int maxHeight)
        {
        }

        public override void Draw(int offsetX, int offsetY)
        {
            int drawX = offsetX + X;
            int drawY = offsetY + Y;
            ConsoleColor fg = IsFocused ? ConsoleColor.Black : ForeColor;
            ConsoleColor bg = IsFocused ? ConsoleColor.White : BackColor;
            string display = $"[{(IsChecked ? 'x' : ' ')}] {Text}";
            if (display.Length > Width) display = display[..Width];

            Renderer.DrawString(drawX, drawY, new string(' ', Width), null, bg);
            Renderer.DrawString(drawX, drawY, display.PadRight(Width), fg, bg);
        }

        public override void Activate() => Toggle();

        public override void HandleKey(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Spacebar)
                Toggle();
        }

        public void Toggle()
        {
            if (!Enabled) return;
            IsChecked = !IsChecked;
            CheckedChanged?.Invoke(IsChecked);
            MarkDirty();
        }
    }
}
