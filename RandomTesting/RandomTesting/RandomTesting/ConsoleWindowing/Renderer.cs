namespace RandomTesting.ConsoleWindowing
{
    // ========== RENDERER ==========
    public static class Renderer
    {
        public static void DrawString(int x, int y, string text,
            ConsoleColor? fore = null, ConsoleColor? back = null)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (y < 0 || y >= Console.WindowHeight) return;
            if (x >= Console.WindowWidth) return;

            if (x < 0)
            {
                int trim = -x;
                if (trim >= text.Length) return;
                text = text[trim..];
                x = 0;
            }

            int maxLength = Console.WindowWidth - x;
            if (maxLength <= 0) return;
            if (text.Length > maxLength)
                text = text[..maxLength];

            var savedFg = Console.ForegroundColor;
            var savedBg = Console.BackgroundColor;
            if (fore.HasValue) Console.ForegroundColor = fore.Value;
            if (back.HasValue) Console.BackgroundColor = back.Value;
            Console.SetCursorPosition(x, y);
            Console.Write(text);
            Console.ForegroundColor = savedFg;
            Console.BackgroundColor = savedBg;
        }

        public static void DrawChar(int x, int y, char ch,
            ConsoleColor? fore = null, ConsoleColor? back = null)
            => DrawString(x, y, ch.ToString(), fore, back);

        public static void DrawHorizontalLine(int x, int y, int length, char ch = '─',
            ConsoleColor? fore = null, ConsoleColor? back = null)
        {
            if (length <= 0) return;
            DrawString(x, y, new string(ch, length), fore, back);
        }

        public static void DrawBox(int x, int y, int width, int height,
            string title = null,
            ConsoleColor borderColor = ConsoleColor.White,
            ConsoleColor titleColor = ConsoleColor.Cyan,
            ConsoleColor bgColor = ConsoleColor.Black)
        {
            if (width <= 0 || height <= 0) return;
            DrawChar(x, y, '┌', borderColor, bgColor);
            int avail = width - 2;
            int titleLen = title?.Length ?? 0;
            if (titleLen > avail) titleLen = avail;
            int leftPad = (avail - titleLen) / 2;

            DrawHorizontalLine(x + 1, y, leftPad, '─', borderColor, bgColor);
            if (titleLen > 0)
            {
                DrawString(x + 1 + leftPad, y, title[..titleLen], titleColor, bgColor);
                DrawHorizontalLine(x + 1 + leftPad + titleLen, y, avail - leftPad - titleLen, '─', borderColor, bgColor);
            }
            else
                DrawHorizontalLine(x + 1, y, avail, '─', borderColor, bgColor);
            DrawChar(x + width - 1, y, '┐', borderColor, bgColor);

            for (int row = 1; row < height - 1; row++)
            {
                DrawChar(x, y + row, '│', borderColor, bgColor);
                DrawChar(x + width - 1, y + row, '│', borderColor, bgColor);
            }

            DrawChar(x, y + height - 1, '└', borderColor, bgColor);
            DrawHorizontalLine(x + 1, y + height - 1, width - 2, '─', borderColor, bgColor);
            DrawChar(x + width - 1, y + height - 1, '┘', borderColor, bgColor);
        }

        public static void ClearArea(int x, int y, int width, int height, ConsoleColor bg = ConsoleColor.Black)
        {
            for (int row = 0; row < height; row++)
                DrawString(x, y + row, new string(' ', width), null, bg);
        }
    }
}
