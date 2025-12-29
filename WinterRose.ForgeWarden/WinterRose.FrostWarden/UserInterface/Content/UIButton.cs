using BulletSharp;
using Raylib_cs;
using WinterRose;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.Recordium;
using WinterRose.WIP.TestClasses;

namespace WinterRose.ForgeWarden.UserInterface;

public class UIButton : UIContent
{
    public RichText Text { get; set; }

    protected UIButton(RichText text) => Text = text;
    protected UIButton(string text) => Text = RichText.Parse(text);

    // Padding and spacing constants
    protected internal const int PaddingX = 12;
    protected internal const int PaddingY = 6;
    protected internal const int Spacing = 10;

    private static readonly VoidInvocation<UIContainer, UIButton> alwaysTrueFunc = Invocation.Create<UIContainer, UIButton>(
        (container, button) =>
        {
            if (container is Toast)
                container.Close();
        });

    private Color backgroundColor;

    public MulticastVoidInvocation<UIContainer, UIButton> OnClick { get; set; } = new();

    public UIButton(string text, VoidInvocation<UIContainer, UIButton>? onClick = null) : this(text)
    {
        OnClick.Subscribe(onClick ?? alwaysTrueFunc);
    }

    public UIButton(RichText text, Action<UIContainer, UIButton> onClick = null) 
        : this(text, onClick is null ? null : Invocation.Create(onClick))
    {

    }

    public UIButton(RichText text, VoidInvocation<UIContainer, UIButton>? onClick = null) : this(text)
    {
        OnClick.Subscribe(onClick ?? alwaysTrueFunc);
    }


    /// <summary>
    /// Invoke the OnClick handler for this button
    /// </summary>
    protected internal override void OnContentClicked(MouseButton button)
    {
        if(button is MouseButton.Left)
            OnClick.Invoke(owner, this);
    }

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
    protected override void Draw(Rectangle btnRect)
    {
        ray.DrawRectangleRec(btnRect, backgroundColor);
        ray.DrawRectangleLinesEx(btnRect, 1, Style.ButtonBorder);

        // compute text area inside padding
        float textMaxWidth = Math.Max(0f, btnRect.Width - PaddingX * 2);
        float textAreaHeight = Math.Max(0f, btnRect.Height - PaddingY * 2);

        // resolve font size the same way CalculateSize does
        int baseSize = Math.Clamp(Style.BaseButtonFontSize, 12, 28);
        int resolvedFontSize = UITextScalar.ResolveFontSize(
            Text,
            baseSize,
            new Rectangle(0, 0, textMaxWidth, float.MaxValue),
            autoScale: true
        );

        // prepare a copy of the RichText with the resolved font size for measurement/drawing
        var textToDraw = Text.Clone();
        textToDraw.FontSize = resolvedFontSize;

        // measure exactly using the renderer so measurement == drawn layout
        var measured = RichTextRenderer.MeasureRichText(textToDraw, textMaxWidth);
        float measuredWidth = measured.Width;
        float measuredHeight = measured.Height;

        // compute text origin — horizontally start at left padding, vertically center the measured text
        float textX = btnRect.X + PaddingX;
        float textY = btnRect.Y + (btnRect.Height - measuredHeight) / 2f;

        // ensure we don't place text outside inner padding bounds
        if (textY < btnRect.Y + PaddingY) textY = btnRect.Y + PaddingY;
        if (textY + measuredHeight > btnRect.Y + btnRect.Height - PaddingY)
            textY = btnRect.Y + btnRect.Height - PaddingY - measuredHeight;

        // draw with exact same max width used for measurement
        RichTextRenderer.DrawRichText(
            textToDraw,
            new Vector2(textX, textY),
            textMaxWidth,
            new Color(255, 255, 255, Style.ContentAlpha),
            Input
        );
    }

    public override Vector2 GetSize(Rectangle availableArea) => CalculateSize(availableArea.Width, Style.BaseButtonFontSize).Size;
    protected internal override float GetHeight(float maxWidth) => GetHeight(maxWidth, Style.BaseButtonFontSize);

    /// <summary>
    /// Calculate the button size for a given width
    /// </summary>
    public virtual Rectangle CalculateSize(float maxWidth, int baseFontScale = 1)
    {
        int baseSize = Math.Clamp(baseFontScale, 12, 28);

        float textMaxWidth = Math.Max(0f, maxWidth - PaddingX * 2);

        int resolvedFontSize = UITextScalar.ResolveFontSize(
            Text,
            baseSize,
            new Rectangle(0, 0, textMaxWidth, float.MaxValue),
            autoScale: true
        );

        var textToMeasure = Text;
        textToMeasure.FontSize = resolvedFontSize;

        Vector2 measured = RichTextRenderer.MeasureRichText(
            textToMeasure,
            textMaxWidth
        ).Size;

        // IMPORTANT:
        // draw vertically centers text inside padded area,
        // so height must fully contain the measured block
        int requiredInnerHeight = (int)MathF.Ceiling(measured.Y);

        int width = (int)MathF.Ceiling(measured.X) + PaddingX * 2;
        int height = requiredInnerHeight + PaddingY * 2;

        return new Rectangle(0, 0, width, height);
    }

    protected internal virtual float GetHeight(float maxWidth, int baseFontScale = 1)
    {
        return CalculateSize(maxWidth, baseFontScale).Height;
    }

    internal static List<Rectangle> LayoutButtons<T>(List<UIButton> buttons, Rectangle bounds, out List<int> rowHeights, int baseFontScale = 1) where T : UIContainer
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
