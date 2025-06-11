using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FrostWarden.DialogBoxes.Boxes;

namespace WinterRose.FrostWarden.DialogBoxes
{
    public class DialogButton
    {
        public string text;
        public Action? OnClick;

        public DialogButton(string text, Action? onClick)
        {
            this.text = text;
            OnClick = onClick;
        }

        internal void Draw(DialogStyle style, Rectangle bounds)
        {
            Vector2 mouse = ray.GetMousePosition();
            bool hovered = ray.CheckCollisionPointRec(mouse, bounds);
            bool mouseDown = ray.IsMouseButtonDown(MouseButton.Left);

            Color backgroundColor;
            if (hovered && mouseDown)
                backgroundColor = style.ButtonClick;
            else if (hovered)
                backgroundColor = style.ButtonHover;
            else
                backgroundColor = style.ButtonBackground;

            ray.DrawRectangleRec(bounds, backgroundColor);
            ray.DrawRectangleLinesEx(bounds, 1, style.ButtonBorder);
            ray.DrawText(text, (int)(bounds.X + 10), (int)(bounds.Y + 7), 16, style.ButtonTextColor);
        }

        internal void Update(Dialog dialog, Rectangle bounds)
        {
            Vector2 mouse = ray.GetMousePosition();
            bool hovered = ray.CheckCollisionPointRec(mouse, bounds);
            bool mouseDown = ray.IsMouseButtonDown(MouseButton.Left);
            bool mouseReleased = ray.IsMouseButtonReleased(MouseButton.Left);

            if (hovered && mouseReleased)
            {
                dialog.IsClosing = true;
                OnClick?.Invoke();
            }
        }
    }
}
