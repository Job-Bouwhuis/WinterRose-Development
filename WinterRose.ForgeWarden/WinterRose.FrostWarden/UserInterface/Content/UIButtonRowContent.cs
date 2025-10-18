using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;

namespace WinterRose.ForgeWarden.UserInterface;

[Obsolete(DiagnosticId = "WR-Use-UIRow-Instead")]
public class UIButtonRowContent : UIContent
{
    private static readonly VoidInvocation<UIContainer, UIButton> alwaysTrueFunc = Invocation.Create<UIContainer, UIButton>(
        (container, button) =>
        {
            if (container is Toast)
                container.Close();
        });
    List<List<Rectangle>> buttonRows = [];

    public List<UIButton> Buttons { get; } = new();

    public float PaddingX { get; set; } = 8;
    public float PaddingY { get; set; } = 4;
    public float Spacing { get; set; } = 6;

    public UIButtonRowContent()
    {

    }

    public UIButtonRowContent(params List<UIButton> buttons) => Buttons = buttons;

    public UIButton AddButton(string text, VoidInvocation<UIContainer, UIButton>? onClick = null)
    {
        var n = new UIButton(text, onClick ?? alwaysTrueFunc)
        {
            owner = owner
        };
        Buttons.Add(n);
        return n;
    }

    public UIButton AddButton(RichText text, VoidInvocation<UIContainer, UIButton>? onClick = null)
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
        List<Rectangle> measuredSizes = new List<Rectangle>();
        // Use the same toast width constant as Draw for consistent scaling
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
            measuredSizes.Add(new Rectangle(0, 0, btnWidth, btnHeight));
        }

        // Layout rows using spacing only between items (no trailing spacing)
        List<float> rowHeights = new List<float>();
        float currentRowWidth = 0;
        float currentRowHeight = 0;
        int itemsInRow = 0;

        foreach (var size in measuredSizes)
        {
            float needed = (itemsInRow == 0) ? size.Width : (Spacing + size.Width);

            if (itemsInRow > 0 && currentRowWidth + needed > width)
            {
                // wrap
                rowHeights.Add(currentRowHeight);
                currentRowWidth = 0;
                currentRowHeight = 0;
                itemsInRow = 0;
                needed = size.Width; // first item in new row
            }

            currentRowWidth += (itemsInRow == 0) ? size.Width : (Spacing + size.Width);
            currentRowHeight = Math.Max(currentRowHeight, size.Height);
            itemsInRow++;
        }

        if (itemsInRow > 0)
            rowHeights.Add(currentRowHeight);

        if (rowHeights.Count == 0)
            return new Vector2(width, 0);

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

        List<Rectangle> measuredSizes = new();
        float baseFontScale = bounds.Width / UIConstants.TOAST_WIDTH;

        // Measure buttons (same logic as GetSize)
        foreach (var button in Buttons)
        {
            int buttonBaseSize = 12;
            int buttonFontSize = (int)(buttonBaseSize * baseFontScale);
            buttonFontSize = Math.Clamp(buttonFontSize, 12, 28);
            button.Text.FontSize = buttonFontSize;

            Rectangle textSize = button.Text.CalculateBounds(bounds.Width);
            int btnWidth = (int)textSize.Width + (int)(PaddingX * 2);
            int btnHeight = (int)textSize.Height + (int)(PaddingY * 2);
            measuredSizes.Add(new Rectangle(0, 0, btnWidth, btnHeight));
        }

        // Build rows: ensure spacing is only between items (no trailing spacing)
        buttonRows = new List<List<Rectangle>>();
        List<float> rowHeights = new List<float>();
        List<Rectangle> currentRow = new List<Rectangle>();
        float currentRowWidth = 0;
        float currentRowHeight = 0;

        for (int i = 0; i < measuredSizes.Count; i++)
        {
            var size = measuredSizes[i];
            float needed = (currentRow.Count == 0) ? size.Width : (currentRowWidth + Spacing + size.Width);

            if (currentRow.Count > 0 && needed > bounds.Width)
            {
                // finalize current row
                buttonRows.Add(currentRow);
                rowHeights.Add(currentRowHeight);

                currentRow = new List<Rectangle>();
                currentRowWidth = 0;
                currentRowHeight = 0;
            }

            // add to current row
            currentRow.Add(size);
            currentRowWidth = (currentRow.Count == 1) ? size.Width : currentRowWidth + Spacing + size.Width;
            currentRowHeight = Math.Max(currentRowHeight, size.Height);
        }

        if (currentRow.Count > 0)
        {
            buttonRows.Add(currentRow);
            rowHeights.Add(currentRowHeight);
        }

        // Draw rows centered horizontally
        float rowY = bounds.Y;
        int buttonIndex = 0;
        for (int r = 0; r < buttonRows.Count; r++)
        {
            var row = buttonRows[r];
            float rowWidth = row.Sum(b => b.Width) + Spacing * (row.Count - 1);
            float rowX = bounds.X + (bounds.Width - rowWidth) / 2f;

            for (int b = 0; b < row.Count; b++)
            {
                var size = row[b];
                Rectangle btnRect = new Rectangle((int)rowX, (int)rowY, size.Width, size.Height);
                // replace size entry with positioned rect so hover logic can use it
                buttonRows[r][b] = btnRect;

                Buttons[buttonIndex++].InternalDraw(btnRect);
                //ray.DrawRectangleLinesEx(btnRect, 1, Color.Magenta);

                rowX += size.Width + Spacing;
            }

            // add spacing between rows but not after the last row
            rowY += rowHeights[r] + (r < buttonRows.Count - 1 ? Spacing : 0);
        }
    }
}
