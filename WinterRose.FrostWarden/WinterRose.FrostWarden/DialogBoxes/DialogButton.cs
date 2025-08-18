using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.DialogBoxes.Boxes;
using WinterRose.ForgeWarden.TextRendering;

namespace WinterRose.ForgeWarden.DialogBoxes
{
    public class DialogButton
    {
        public RichText text;
        public Action? OnClick;

        public DialogButton(string text, Action? onClick)
        {
            this.text = RichText.Parse(text);
            OnClick = onClick;
        }

        internal void Draw(Dialog dialog, DialogStyle style, Rectangle bounds)
        {
            Vector2 mouse = ray.GetMousePosition();
            bool hovered = ray.CheckCollisionPointRec(mouse, bounds);
            bool mouseDown = ray.IsMouseButtonDown(MouseButton.Left);
            bool mouseReleased = ray.IsMouseButtonReleased(MouseButton.Left);

            Color backgroundColor;
            if (hovered && mouseDown)
                backgroundColor = style.ButtonClick;
            else if (hovered)
                backgroundColor = style.ButtonHover;
            else
                backgroundColor = style.ButtonBackground;

            ray.DrawRectangleRec(bounds, backgroundColor);
            ray.DrawRectangleLinesEx(bounds, 1, style.ButtonBorder);
            RichTextRenderer.DrawRichText(text, new(bounds.X + 10, bounds.Y + 7), bounds.Width);

            if (hovered && mouseReleased)
            {
                dialog.IsClosing = true;
                OnClick?.Invoke();
            }
        }
    }
}
