using Raylib_cs;
using WinterRose;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.WIP.TestClasses;

namespace WinterRose.ForgeWarden.UserInterface.Content;

public delegate bool ButtonClickHandler(UIContainer container, UIButton button);

public class UIButton : UIContent
{
    public RichText Text { get; set; }

    protected UIButton(RichText text) => Text = text;
    protected UIButton(string text) => Text = RichText.Parse(text);

    // Padding and spacing constants
    protected internal const int PaddingX = 12;
    protected internal const int PaddingY = 6;
    protected internal const int Spacing = 10;

    private static readonly ButtonClickHandler alwaysTrueFunc = (t, b) => true;
    private Color backgroundColor;

    public ButtonClickHandler OnClick { get; set; }

    public UIButton(string text, ButtonClickHandler? onClick = null) : this(text)
    {
        OnClick = onClick ?? alwaysTrueFunc;
    }

    public UIButton(RichText text, ButtonClickHandler? onClick = null) : this(text)
    {
        OnClick = onClick ?? alwaysTrueFunc;
    }


    /// <summary>
    /// Invoke the OnClick handler for this button
    /// </summary>
    protected internal override void OnContentClicked(MouseButton button) => OnClick(owner, this);

    protected internal override void Update()
    {
        if (IsHovered && Input.IsDown(MouseButton.Left))
            backgroundColor = Style.ButtonClick;
        else if (IsHovered)
            backgroundColor = Style.ButtonHover;
        else
            backgroundColor = Style.ButtonBackground;
    }

    /// <summary>
    /// Draws the button
    /// </summary>
    /// <param name="btnRect"></param>
    /// <returns>True when the button has been clicked</returns>
    internal protected override void Draw(Rectangle btnRect)
    {
        ray.DrawRectangleRec(btnRect, backgroundColor);
        ray.DrawRectangleLinesEx(btnRect, 1, Style.ButtonBorder);

        RichTextRenderer.DrawRichText(
            Text,
            new(btnRect.X + 10, btnRect.Y + 7),
            btnRect.Width,
            new Color(255, 255, 255, Style.ContentAlpha),
            Input);
    }

    public override Vector2 GetSize(Rectangle availableArea) => CalculateSize(availableArea.Width, Style.BaseButtonFontSize).Size;
    protected internal override float GetHeight(float maxWidth) => GetHeight(maxWidth, Style.BaseButtonFontSize);

    /// <summary>
    /// Calculate the button size for a given width
    /// </summary>
    public virtual Rectangle CalculateSize(float maxWidth, float baseFontScale = 1f)
    {
        int buttonBaseSize = 12;
        int buttonFontSize = (int)(buttonBaseSize * baseFontScale);
        buttonFontSize = Math.Clamp(buttonFontSize, 12, 28);
        Text.FontSize = buttonFontSize;

        Rectangle textSize = Text.CalculateBounds(maxWidth);
        int btnWidth = (int)textSize.Width + PaddingX * 2;
        int btnHeight = (int)textSize.Height + PaddingY * 2;

        return new Rectangle(0, 0, btnWidth, btnHeight);
    }

    protected internal virtual float GetHeight(float maxWidth, float baseFontScale = 1f)
    {
        return CalculateSize(maxWidth, baseFontScale).Height;
    }

    internal static List<Rectangle> LayoutButtons<T>(List<UIButton> buttons, Rectangle bounds, out List<int> rowHeights, float baseFontScale = 1f) where T : UIContainer
    {
        List<Rectangle> buttonSizes = buttons.Select(b => b.CalculateSize(bounds.Width, baseFontScale)).ToList();
        List<List<Rectangle>> rows = new();
        rowHeights = new List<int>();

        List<Rectangle> currentRow = new();
        float xPos = bounds.X;
        int currentRowHeight = 0;

        for (int i = 0; i < buttons.Count; i++)
        {
            Rectangle size = buttonSizes[i];

            if (xPos + size.Width > bounds.X + bounds.Width)
            {
                rows.Add(currentRow);
                rowHeights.Add(currentRowHeight);

                currentRow = new List<Rectangle>();
                xPos = bounds.X;
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

        // Calculate final positions for each button
        List<Rectangle> positionedButtons = new();
        float rowY = bounds.Y;

        foreach (var row in rows)
        {
            float rowWidth = row.Sum(r => r.Width) + Spacing * (row.Count - 1);
            float rowX = bounds.X + (bounds.Width - rowWidth) / 2;

            foreach (var size in row)
            {
                positionedButtons.Add(new Rectangle((int)rowX, (int)rowY, size.Width, size.Height));
                rowX += size.Width + Spacing;
            }

            rowY += rowHeights[rows.IndexOf(row)] + Spacing;
        }

        return positionedButtons;
    }
}
