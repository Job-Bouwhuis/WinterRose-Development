using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FrostWarden
{
    using Raylib_cs;
    using System.Numerics;
    using WinterRose.FrostWarden.TextRendering;

    public class UIContext
    {
        private Vector2 position;
        private float lineHeight = 24f;
        private float padding = 6f;
        private float labelWidth = 140f;
        private float contentWidth = 200f;

        private Color tintColor = Color.White; // default to no tint

        public void Begin(Vector2 startPos, Color tint)
        {
            position = startPos;
            tintColor = tint;
        }

        // End stays the same
        public void End() { }

        public void Label(string text)
        {
            Color tinted = MultiplyColor(Color.White, tintColor);
            RichTextRenderer.DrawRichText(
                RichText.Parse(text, Color.White), 
                new Vector2(position.X, position.Y), 
                null, 
                20, 
                Raylib.GetScreenWidth(), 
                tinted);
            position.Y += lineHeight;
        }

        public bool Button(string text)
        {
            RichText t = RichText.Parse(text, MultiplyColor(Color.White, tintColor));
            float width = t.MeasureText(null, 20);
            Rectangle rect = new Rectangle(position.X, position.Y, width + contentWidth, lineHeight);
            bool pressed = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect) &&
                           Raylib.IsMouseButtonPressed(MouseButton.Left);

            Color baseColor = pressed ? Color.DarkBlue : Color.Blue;
            Raylib.DrawRectangleRec(rect, MultiplyColor(baseColor, tintColor));
            RichTextRenderer.DrawRichText(
                t,
                new Vector2(position.X + 10, position.Y + 2),
                null,
                20,
                Raylib.GetScreenWidth(),
                tintColor);
            position.Y += lineHeight + padding;
            return pressed;
        }

        public bool Checkbox(string label, bool value)
        {
            Rectangle box = new Rectangle(position.X, position.Y, 20, 20);
            bool clicked = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), box) &&
                           Raylib.IsMouseButtonPressed(MouseButton.Left);

            if (clicked) value = !value;

            Raylib.DrawRectangleRec(box, MultiplyColor(Color.LightGray, tintColor));
            if (value) Raylib.DrawText("X", (int)(box.X + 4), (int)(box.Y + 2), 20, MultiplyColor(Color.Black, tintColor));

            RichTextRenderer.DrawRichText(
                RichText.Parse(label, MultiplyColor(Color.White, tintColor)),
                new Vector2(box.X + 30, box.Y),
                null,
                20,
                Raylib.GetScreenWidth(),
                tintColor);
            position.Y += lineHeight + padding;
            return value;
        }

        public float Slider(string label, float value, float min, float max)
        {
            Label(label); // Draw label first

            float sliderX = position.X;
            float sliderY = position.Y;
            float width = labelWidth + contentWidth;
            float height = 10;

            Rectangle sliderBar = new Rectangle(sliderX, sliderY, width, height);
            Raylib.DrawRectangleRec(sliderBar, MultiplyColor(Color.Gray, tintColor));

            float percent = (value - min) / (max - min);
            float knobX = sliderX + percent * width;

            Rectangle knob = new Rectangle(knobX - 5, sliderY - 5, 10, 20);
            Raylib.DrawRectangleRec(knob, MultiplyColor(Color.Yellow, tintColor));

            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), sliderBar) &&
                Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                float mouseX = Raylib.GetMouseX();
                float newPercent = (mouseX - sliderX) / width;
                value = Math.Clamp(min + newPercent * (max - min), min, max);
            }

            position.Y += lineHeight + padding;
            return value;
        }

        private Color MultiplyColor(Color c1, Color c2)
        {
            return new Color(
                (byte)(c1.R * c2.R / 255),
                (byte)(c1.G * c2.G / 255),
                (byte)(c1.B * c2.B / 255),
                (byte)(c1.A * c2.A / 255)
            );
        }
    }

}
