using Raylib_cs;
using WinterRose.FrostWarden.TextRendering;

namespace WinterRose.FrostWarden.DialogBoxes.Boxes
{
    public abstract class Dialog
    {
        private Rectangle btn;

        public RichText Title { get; set; }
        public RichText Message { get; set; }
        public DialogPlacement Placement { get; set; }
        public DialogPriority Priority { get; }

        public float AnimationTimeIn { get; internal set; }
        public float AnimationTimeOut { get; internal set; }
        public float ContentAlphaTime { get; internal set; } = 0f;

        public bool IsClosing { get; internal set; }

        public Action<UIContext>? OnImGui { get; set; }

        private readonly int buttonWidth;
        private readonly int buttonHeight;
        private readonly int spacing;

        public DialogButton[] Buttons { get; }

        public bool IsVisible => !IsClosing;

        public DialogStyle Style { get; set; } = new();
        public float YAnimateTime { get; internal set; }

        protected Dialog(
            string title,
            string message,
            DialogPlacement placement,
            DialogPriority priority,
            string[]? buttons,
            Action[]? onButtonClick,
            Action<UIContext>? onImGui)
        {
            Title = RichText.Parse(title, Color.White);
            Message = RichText.Parse(message, Color.White);
            Placement = placement;
            Priority = priority;
            AnimationTimeIn = 0f;
            AnimationTimeOut = 0f;
            ContentAlphaTime = 0f;

            buttons ??= [];
            onButtonClick ??= [];

            Buttons = new DialogButton[buttons.Length];

            for (int i = 0; i < buttons.Length; i++)
            {
                string label = buttons[i];
                Action? onClick = null;
                if (onButtonClick.Length < i)
                    onClick = onButtonClick[i]!;

                Buttons[i] = new DialogButton(label, onClick);
            }

            IsClosing = false;
            OnImGui = onImGui;

            buttonWidth = 80;
            buttonHeight = 30;
            spacing = 10;
        }

        public virtual void Close()
        {
            IsClosing = true;
        }

        internal void RenderBox(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth, ref int y)
        {
            RichTextRenderer.DrawRichText(Title, new((int)bounds.X + padding, y), null, 25, innerWidth, Style.ContentColor);
            y += 35;

            RichTextRenderer.DrawRichText(Message, new((int)bounds.X + padding, y), null, 16, innerWidth, Style.ContentColor);
            y += 40;

            DrawContent(bounds, contentAlpha, ref padding, ref innerWidth, ref y);

            float totalButtonWidth = Buttons.Length * buttonWidth + (Buttons.Length - 1) * spacing;
            float startX = bounds.X + (bounds.Width - totalButtonWidth) / 2;

            for (int i = 0; i < Buttons.Length; i++)
            {
                btn = new Rectangle(startX + i * (buttonWidth + spacing), y, buttonWidth, buttonHeight);
                Buttons[i].Draw(Style, btn);
            }
            y += 25;

            UIContext c = new UIContext();
            c.Begin(new Vector2(bounds.X, y), Style.ContentColor);
            OnImGui?.Invoke(c);
            c.End();
        }

        internal void UpdateBox()
        {
            for (int i = 0; i < Buttons.Length; i++)
                Buttons[i].Update(this, btn);

            Update();
        }

        public abstract void DrawContent(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth, ref int y);
        public abstract void Update();
    }
}