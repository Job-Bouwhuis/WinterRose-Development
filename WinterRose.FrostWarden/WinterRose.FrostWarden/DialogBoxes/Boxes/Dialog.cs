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

        public float AnimationTimeIn { get; internal set; }
        public float AnimationTimeOut { get; internal set; }
        public float ContentAlphaTime { get; internal set; } = 0f;

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

        protected Dialog(
            string title,
            string message,
            DialogPlacement placement,
            DialogPriority priority,
            string[]? buttons,
            Func<bool>[]? onButtonClick,
            Action<UIContext>? onImGui)
        {
            Title = RichText.Parse(title, Color.White);
            Title.FontSize = 25;

            Message = RichText.Parse(message, Color.White);
            Message.FontSize = 16;

            Placement = placement;
            Priority = priority;
            AnimationTimeIn = 0f;
            AnimationTimeOut = 0f;
            ContentAlphaTime = 0f;

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
        }

        internal void RenderBox(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth, ref int y)
        {
            RichTextRenderer.DrawRichText(Title, new((int)bounds.X + padding, y), innerWidth, Style.ContentColor);
            y += 35 + (int)Title.CalculateBounds(innerWidth).Height;

            RichTextRenderer.DrawRichText(Message, new((int)bounds.X + padding, y), innerWidth, Style.ContentColor);
            y += 40 + (int)Message.CalculateBounds(innerWidth).Height;

            DrawContent(bounds, contentAlpha, ref padding, ref innerWidth, ref y);

            float totalButtonWidth = 0;
            buttonSizes.Clear();
            for (int i = 0; i < Buttons.Count; i++)
            {
                Rectangle textSize = Buttons[i].text.CalculateBounds(innerWidth);
                int btnWidth = (int)textSize.Width + paddingX * 2;
                int btnHeight = (int)textSize.Height + paddingY * 2;
                buttonSizes.Add(new Rectangle(0, 0, btnWidth, btnHeight));

                totalButtonWidth += btnWidth;
                if (i < Buttons.Count - 1)
                    totalButtonWidth += spacing;
            }


            float startX = bounds.X + (bounds.Width - totalButtonWidth) / 2;

            int yIncreaseAfterButtons = 25;
            float x = startX;

            for (int i = 0; i < Buttons.Count; i++)
            {
                Rectangle size = buttonSizes[i];
                Rectangle btn = new((int)x, y, size.Width, size.Height);

                if (size.Height > yIncreaseAfterButtons)
                    yIncreaseAfterButtons = (int)size.Height;

                Buttons[i].Draw(this, Style, btn);
                x += size.Width + spacing;
            }

            y += yIncreaseAfterButtons;


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