using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;

namespace WinterRose.ForgeWarden.UserInterface;

public class ButtonRowContent : UIContent
{
    private static readonly ButtonClickHandler alwaysTrueFunc = (container, button) => container.Close();
    List<List<Rectangle>> buttonRows = [];

    public List<UIButton> Buttons { get; } = new();

    public float PaddingX { get; set; } = 8;
    public float PaddingY { get; set; } = 4;
    public float Spacing { get; set; } = 6;

    public ButtonRowContent()
    {

    }

    public ButtonRowContent(params List<UIButton> buttons) => Buttons = buttons;

    public UIButton AddButton(string text, ButtonClickHandler? onClick = null)
    {
        var n = new UIButton(text, onClick ?? alwaysTrueFunc)
        {
            owner = owner
        };
        Buttons.Add(n);
        return n;
    }

    public UIButton AddButton(RichText text, ButtonClickHandler? onClick = null)
    {
        var n = new UIButton(text, onClick ?? alwaysTrueFunc)
        {
            owner = owner
        };
        Buttons.Add(n);
        return n;
    }

    protected internal override void OnHover()
    {
        if (buttonRows.Sum(r => r.Count) != Buttons.Count)
            return;

        List<Rectangle> buttonSizes = buttonRows.SelectMany(r => r).ToList();

        for (int i = 0; i < Buttons.Count; i++)
        {
            UIButton btn = Buttons[i];
            Rectangle r = buttonSizes[i];

            if (btn.IsContentHovered(r))
            {
                btn.IsHovered = true;
                btn.OnHover();
            }
            else
            {
                if (btn.IsHovered)
                    btn.OnHoverEnd();
                btn.IsHovered = false;
            }
        }
    }

    protected internal override void OnContentClicked(MouseButton button)
    {
        foreach (var btn in Buttons)
        {
            if (btn.IsHovered)
            {
                btn.OnContentClicked(button);
                break;
            }
        }
    }

    protected internal override void OnHoverEnd()
    {
        foreach (var btn in Buttons)
            btn.IsHovered = false;
    }

    protected internal override void OnOwnerClosing()
    {
        foreach (var n in Buttons)
            n.OnOwnerClosing();
    }

    public override Vector2 GetSize(Rectangle availableSize)
    {
        if (Buttons.Count == 0)
            return Vector2.Zero;

        float width = availableSize.Width;
        List<Rectangle> buttonSizes = new();
        float baseFontScale = width / UIConstants.TOAST_WIDTH;

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

        if (currentRowHeight > 0)
            rowHeights.Add(currentRowHeight);

        // Total height = sum of rows + spacing
        float totalHeight = rowHeights.Sum() + Spacing * (rowHeights.Count - 1);
        return new Vector2(width, totalHeight);
    }

    protected internal override float GetHeight(float maxWidth)
        => GetSize(new Rectangle(0, 0, maxWidth, int.MaxValue)).Y;

    protected internal override void Update()
    {
        foreach (var button in Buttons)
        {
            button.Update();
        }
    }

    protected override void Draw(Rectangle bounds)
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
        buttonRows = new();
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
                buttonRows.Add(currentRow);
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
            buttonRows.Add(currentRow);
            rowHeights.Add(currentRowHeight);
        }

        // Draw rows
        float rowY = bounds.Y;
        int buttonIndex = 0;

        for (int r = 0; r < buttonRows.Count; r++)
        {
            float rowWidth = buttonRows[r].Sum(b => b.Width) + Spacing * (buttonRows[r].Count - 1);
            float rowX = bounds.X + (bounds.Width - rowWidth) / 2f; // center row

            for (int b = 0; b < buttonRows[r].Count; b++)
            {
                Rectangle btnRect = buttonRows[r][b] = new((int)rowX, (int)rowY, buttonRows[r][b].Width, buttonRows[r][b].Height);

                Buttons[buttonIndex++].InternalDraw(btnRect);
                rowX += buttonRows[r][b].Width + Spacing;
            }

            rowY += rowHeights[r] + Spacing;
        }
    }
}
