using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.TextRendering;

namespace WinterRose.ForgeWarden.ToastNotifications;

public class ToastButton
{
    private static readonly Func<Toast, ToastButton, bool> alwaysTrueFunc = (t, b) => true;

    public RichText Text;
    public Func<Toast, ToastButton, bool> OnClick;

    public ToastButton(string text, Func<Toast, ToastButton, bool> onClick)
    {
        Text = RichText.Parse(text);
        OnClick = onClick;
    }

    public ToastButton(string text)
    {
        Text = RichText.Parse(text);
        OnClick = alwaysTrueFunc;
    }

    public ToastButton(RichText text, Func<Toast, ToastButton, bool> onClick)
    {
        Text = text;
        OnClick = onClick;
    }

    public ToastButton(RichText text)
    {
        Text = text;
        OnClick = alwaysTrueFunc;
    }
}

public class ToastButtonContent : ToastContent
{
    private static readonly Func<Toast, ToastButton, bool> alwaysTrueFunc = (t, b) => true;

    public List<ToastButton> Buttons { get; } = new();

    public float PaddingX { get; set; } = 8;
    public float PaddingY { get; set; } = 4;
    public float Spacing { get; set; } = 6;

    public void AddButton(string text, Func<Toast, ToastButton, bool>? onClick = null)
    {
        Buttons.Add(new ToastButton(text, onClick ?? alwaysTrueFunc));
    }

    public void AddButton(RichText text, Func<Toast, ToastButton, bool>? onClick = null)
    {
        Buttons.Add(new ToastButton(text, onClick ?? alwaysTrueFunc));
    }

    public override float GetHeight(float width)
    {
        if (Buttons.Count == 0) return 0;

        List<Rectangle> buttonSizes = new();
        float baseFontScale = width / Toasts.TOAST_WIDTH;

        // Measure buttons
        foreach (var button in Buttons)
        {
            int buttonBaseSize = 12;
            int buttonFontSize = (int)(buttonBaseSize * baseFontScale);
            buttonFontSize = Math.Clamp(buttonFontSize, 12, 28);
            button.Text.FontSize = buttonFontSize;

            Rectangle textSize = button.Text.CalculateBounds(width);
            int btnWidth = (int)textSize.Width + (int)(PaddingX * 2);
            int btnHeight = (int)textSize.Height + (int)(PaddingY * 2);
            buttonSizes.Add(new Rectangle(0, 0, btnWidth, btnHeight));
        }

        // Layout rows
        List<int> rowHeights = new();
        float xPos = 0;
        int currentRowHeight = 0;

        foreach (var size in buttonSizes)
        {
            if (xPos + size.Width > width)
            {
                rowHeights.Add(currentRowHeight);
                xPos = 0;
                currentRowHeight = 0;
            }

            currentRowHeight = (int)Math.Max(currentRowHeight, size.Height);
            xPos += size.Width + Spacing;
        }

        if (currentRowHeight > 0) rowHeights.Add(currentRowHeight);

        // Total height = sum of rows + spacing
        float totalHeight = rowHeights.Sum() + Spacing * (rowHeights.Count - 1);
        return totalHeight;
    }

    public override void Draw(Rectangle bounds, float contentAlpha)
    {
        if (Buttons.Count == 0) return;

        List<Rectangle> buttonSizes = new();
        float baseFontScale = bounds.Width / Toasts.TOAST_WIDTH;

        // Measure buttons
        foreach (var button in Buttons)
        {
            int buttonBaseSize = 12;
            int buttonFontSize = (int)(buttonBaseSize * baseFontScale);
            buttonFontSize = Math.Clamp(buttonFontSize, 12, 28);
            button.Text.FontSize = buttonFontSize;

            Rectangle textSize = button.Text.CalculateBounds(bounds.Width);
            int btnWidth = (int)textSize.Width + (int)(PaddingX * 2);
            int btnHeight = (int)textSize.Height + (int)(PaddingY * 2);
            buttonSizes.Add(new Rectangle(0, 0, btnWidth, btnHeight));
        }

        // Layout rows
        List<List<Rectangle>> rows = new();
        List<int> rowHeights = new();
        float xPos = bounds.X;
        float yPos = bounds.Y;
        int currentRowHeight = 0;
        List<Rectangle> currentRow = new();

        for (int i = 0; i < Buttons.Count; i++)
        {
            var size = buttonSizes[i];
            if (xPos + size.Width > bounds.X + bounds.Width)
            {
                rows.Add(currentRow);
                rowHeights.Add(currentRowHeight);

                currentRow = new List<Rectangle>();
                xPos = bounds.X;
                yPos += currentRowHeight + Spacing;
                currentRowHeight = 0;
            }

            currentRow.Add(size);
            currentRowHeight = (int)Math.Max(currentRowHeight, size.Height);
            xPos += size.Width + Spacing;
        }

        if (currentRow.Count > 0)
        {
            rows.Add(currentRow);
            rowHeights.Add(currentRowHeight);
        }

        // Draw rows
        float rowY = bounds.Y;
        int buttonIndex = 0;

        for (int r = 0; r < rows.Count; r++)
        {
            float rowWidth = rows[r].Sum(b => b.Width) + Spacing * (rows[r].Count - 1);
            float rowX = bounds.X + (bounds.Width - rowWidth) / 2f; // center row

            for (int b = 0; b < rows[r].Count; b++)
            {
                Rectangle btnRect = new((int)rowX, (int)rowY, rows[r][b].Width, rows[r][b].Height);
                DrawButton(Buttons[buttonIndex++], btnRect);
                rowX += rows[r][b].Width + Spacing;
            }

            rowY += rowHeights[r] + Spacing;
        }
    }

    private void DrawButton(ToastButton button, Rectangle bounds)
    {
        Vector2 mouse = ray.GetMousePosition();
        bool hovered = ray.CheckCollisionPointRec(mouse, bounds);
        bool mouseDown = ray.IsMouseButtonDown(MouseButton.Left);
        bool mouseReleased = ray.IsMouseButtonReleased(MouseButton.Left);

        Color backgroundColor;
        if (hovered && mouseDown)
            backgroundColor = Style.ButtonClick;
        else if (hovered)
            backgroundColor = Style.ButtonHover;
        else
            backgroundColor = Style.ButtonBackground;

        ray.DrawRectangleRec(bounds, backgroundColor);
        ray.DrawRectangleLinesEx(bounds, 1, Style.ButtonBorder);

        RichTextRenderer.DrawRichText(button.Text, new(bounds.X + 10, bounds.Y + 7), bounds.Width, new Color(255, 255, 255, Style.contentAlpha));

        if (hovered && mouseReleased && !owner.IsMorphDrawing)
        {
            button.OnClick?.Invoke(owner, button);
        }
    }
}
