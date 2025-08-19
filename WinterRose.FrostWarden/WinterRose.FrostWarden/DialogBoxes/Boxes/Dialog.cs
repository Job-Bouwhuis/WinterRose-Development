using Raylib_cs;
using WinterRose.ForgeWarden.TextRendering;

namespace WinterRose.ForgeWarden.DialogBoxes.Boxes
{
    public abstract class Dialog
    {
        List<Rectangle> buttonSizes = [];

        public RichText Title { get; set; }
        public RichText Message { get; set; }
        public DialogPlacement Placement { get; set; }
        public DialogPriority Priority { get; }

        public Rectangle Bounds => Dialogs.GetDialogBounds(Placement);

        public bool IsClosing { get; internal set; }

        public Action<UIContext>? OnImGui { get; set; }

        private const int spacing = 10;
        private const int paddingX = 12;
        private const int paddingY = 6;

        public List<DialogButton> Buttons { get; }

        public bool IsVisible => !IsClosing;

        public DialogStyle Style { get; set; } = new();
        public float YAnimateTime { get; internal set; }
        internal bool WasBumped { get; set; }
        public DialogAnimation CurrentAnim { get; internal set; } = new() { Elapsed = 0f };

        protected Dialog(
            string title,
            string message,
            DialogPlacement placement,
            DialogPriority priority,
            string[]? buttons,
            Func<bool>[]? onButtonClick,
            Action<UIContext>? onImGui)
        {
            Rectangle bounds = Dialogs.GetDialogBounds(Placement);
            float scaleRef = Math.Min(bounds.Width, bounds.Height);
            float titleScale = scaleRef * 0.09f;
            float messageScale = scaleRef * 0.04f;

            Title = RichText.Parse(title, Color.White);
            Title.FontSize = (int)Math.Clamp(titleScale, 14, 36);

            Message = RichText.Parse(message, Color.White);
            Message.FontSize = (int)Math.Clamp(messageScale, 10, 24);


            Placement = placement;
            Priority = priority;

            buttons ??= [];
            onButtonClick ??= [];

            Buttons = new List<DialogButton>(buttons.Length);

            for (int i = 0; i < buttons.Length; i++)
            {
                string label = buttons[i];
                Func<bool>? onClick = null;
                if (onButtonClick.Length < i)
                    onClick = onButtonClick[i]!;

                Buttons.Add(new DialogButton(label, onClick));
            }

            IsClosing = false;
            OnImGui = onImGui;
        }

        public virtual void Close()
        {
            IsClosing = true;
            CurrentAnim = CurrentAnim with
            {
                Elapsed = 0,
                Completed = false
            };
        }

        internal void RenderBox(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth, ref int y)
        {
            // Scale Title + Message fonts based on dialog size
            float baseFontScale = (bounds.Height / 600f + bounds.Width / 800f) / 2f; // avg scale from height+width

            RichTextRenderer.DrawRichText(Title, new((int)bounds.X + padding, y), innerWidth, Style.ContentColor);
            y += 35 + (int)Title.CalculateBounds(innerWidth).Height;

            RichTextRenderer.DrawRichText(Message, new((int)bounds.X + padding, y), innerWidth, Style.ContentColor);
            y += 40 + (int)Message.CalculateBounds(innerWidth).Height;

            DrawContent(bounds, contentAlpha, ref padding, ref innerWidth, ref y);


            // --- Buttons ---
            buttonSizes.Clear();

            // scale button font size similarly
            int buttonBaseSize = 12;
            int buttonFontSize = (int)(buttonBaseSize * baseFontScale);
            buttonFontSize = Math.Clamp(buttonFontSize, 12, 28); // keep sane range

            for (int i = 0; i < Buttons.Count; i++)
            {
                Buttons[i].text.FontSize = buttonFontSize; // apply scale

                Rectangle textSize = Buttons[i].text.CalculateBounds(innerWidth);
                int btnWidth = (int)textSize.Width + paddingX * 2;
                int btnHeight = (int)textSize.Height + paddingY * 2;
                buttonSizes.Add(new Rectangle(0, 0, btnWidth, btnHeight));
            }

            // Collect rows
            List<List<Rectangle>> rows = new();
            List<int> rowHeights = new();
            float xPos = bounds.X;
            int currentRowHeight = 0;
            List<Rectangle> currentRow = new();

            for (int i = 0; i < Buttons.Count; i++)
            {
                Rectangle size = buttonSizes[i];

                if (xPos + size.Width > bounds.X + bounds.Width)
                {
                    rows.Add(currentRow);
                    rowHeights.Add(currentRowHeight);

                    currentRow = new List<Rectangle>();
                    xPos = bounds.X;
                    y += currentRowHeight + spacing;
                    currentRowHeight = 0;
                }

                currentRow.Add(size);
                currentRowHeight = (int)Math.Max(currentRowHeight, size.Height);
                xPos += size.Width + spacing;
            }

            if (currentRow.Count > 0)
            {
                rows.Add(currentRow);
                rowHeights.Add(currentRowHeight);
            }

            // Calculate total button block height
            int totalButtonHeight = rowHeights.Sum() + spacing * (rowHeights.Count - 1);

            // If buttons overflow dialog bottom, shift them up
            int buttonsY = y;
            if (buttonsY + totalButtonHeight > bounds.Y + bounds.Height)
            {
                buttonsY = (int)bounds.Y + (int)bounds.Height - totalButtonHeight;
            }

            // Draw rows
            float rowY = buttonsY;
            int buttonIndex = 0;
            for (int r = 0; r < rows.Count; r++)
            {
                float rowWidth = rows[r].Sum(b => b.Width) + spacing * (rows[r].Count - 1);
                float rowX = bounds.X + (bounds.Width - rowWidth) / 2; // center row

                for (int b = 0; b < rows[r].Count; b++)
                {
                    Rectangle btnRect = new((int)rowX, (int)rowY, rows[r][b].Width, rows[r][b].Height);
                    Buttons[buttonIndex++].Draw(this, Style, btnRect);
                    rowX += rows[r][b].Width + spacing;
                }

                rowY += rowHeights[r] + spacing;
            }

            y = (int)rowY; 

            // --- ImGui / Additional UI ---
            UIContext c = new UIContext();
            c.Begin(new Vector2(bounds.X, y), Style.ContentColor);
            OnImGui?.Invoke(c);
            c.End();
        }


        internal void UpdateBox()
        {
            Update();
        }

        public abstract void DrawContent(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth, ref int y);
        public abstract void Update();
    }
}