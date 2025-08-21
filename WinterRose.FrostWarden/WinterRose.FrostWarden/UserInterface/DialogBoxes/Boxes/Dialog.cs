using Raylib_cs;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes.Enums;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace WinterRose.ForgeWarden.UserInterface.DialogBoxes;

public class Dialog : UIContainer
{
    public DialogPlacement Placement { get; set; }
    public DialogPriority Priority { get; }

    public Rectangle DialogPlacementBounds => Dialogs.GetDialogBounds(Placement);

    public const float DIALOG_CONTENT_PADDING = 4;

    public float YAnimateTime { get; internal set; }
    internal bool WasBumped { get; set; }
    public DialogAnimation CurrentAnim { get; internal set; } = new() { Elapsed = 0f };

    public override InputContext Input => Dialogs.Input;

    /// <summary> Used for Toast-to-Dialog morph </summary>
    internal bool DrawContentOnly { get; set; }

    public RichText Title { get; private set; }

    public Dialog(
        string title,
        string message,
        DialogPlacement placement = DialogPlacement.CenterSmall,
        DialogPriority priority = DialogPriority.Normal)
    {
        Placement = placement;
        Priority = priority;

        Style = new DialogStyle();

        SetupTitle(title);
        SetupMessage(message);
    }

    public Dialog(
    string title,
    DialogPlacement placement,
    DialogPriority priority)
    {
        Placement = placement;
        Priority = priority;

        Style = new DialogStyle();

        SetupTitle(title);
    }

    protected virtual void SetupTitle(string title)
    {
        Rectangle bounds = Dialogs.GetDialogBounds(Placement);
        float scaleRef = Math.Min(bounds.Width, bounds.Height);
        float titleScale = scaleRef * 0.09f;

        UIMessageContent titleContent = new UIMessageContent(title, UIFontSizePreset.Title);
        titleContent.Text.FontSize = (int)Math.Clamp(titleScale, 14, 36);

        titleContent.owner = this;
        Contents.Insert(0, titleContent);
        Title = titleContent.Text;
    }

    protected virtual void SetupMessage(string message)
    {
        Rectangle bounds = Dialogs.GetDialogBounds(Placement);
        float scaleRef = Math.Min(bounds.Width, bounds.Height);
        float messageScale = scaleRef * 0.04f;

        UIMessageContent messageContent = new UIMessageContent(message, UIFontSizePreset.Message);
        messageContent.Text.FontSize = (int)Math.Clamp(messageScale, 10, 24);

        messageContent.owner = this;
        Contents.Insert(1, messageContent);
    }

    public override void Close()
    {
        base.Close();
        CurrentAnim = CurrentAnim with { Elapsed = 0, Completed = false };
    }

    ///// <summary>
    ///// Draws the default buttons. Subclasses can override if they need something custom.
    ///// </summary>
    //protected virtual void DrawButtons(Rectangle bounds, float baseFontScale, ref int y)
    //{
    //    buttonSizes.Clear();

    //    int buttonBaseSize = 12;
    //    int buttonFontSize = Math.Clamp((int)(buttonBaseSize * baseFontScale), 12, 28);

    //    // measure buttons
    //    foreach (var button in Buttons)
    //    {
    //        button.text.FontSize = buttonFontSize;
    //        Rectangle size = button.text.CalculateBounds(bounds.Width);
    //        buttonSizes.Add(new Rectangle(0, 0, size.Width + 12 * 2, size.Height + 6 * 2));
    //    }

    //    // layout rows
    //    var (rows, rowHeights) = LayoutButtons(bounds, buttonSizes);

    //    // compute total height
    //    int totalButtonHeight = rowHeights.Sum() + 10 * (rowHeights.Count - 1);
    //    int buttonsY = Math.Min(y, (int)bounds.Y + (int)bounds.Height - totalButtonHeight);

    //    // draw
    //    float rowY = buttonsY;
    //    int buttonIndex = 0;
    //    for (int r = 0; r < rows.Count; r++)
    //    {
    //        float rowWidth = rows[r].Sum(b => b.Width) + 10 * (rows[r].Count - 1);
    //        float rowX = bounds.X + (bounds.Width - rowWidth) / 2;

    //        foreach (var rect in rows[r])
    //        {
    //            Rectangle btnRect = new((int)rowX, (int)rowY, rect.Width, rect.Height);
    //            Buttons[buttonIndex++].Draw(this, Style, btnRect);
    //            rowX += rect.Width + 10;
    //        }
    //        rowY += rowHeights[r] + 10;
    //    }

    //    y = (int)rowY;
    //}

    //private static (List<List<Rectangle>> rows, List<int> rowHeights) LayoutButtons(Rectangle bounds, List<Rectangle> sizes)
    //{
    //    var rows = new List<List<Rectangle>>();
    //    var heights = new List<int>();

    //    float xPos = bounds.X;
    //    int currentRowHeight = 0;
    //    var currentRow = new List<Rectangle>();

    //    foreach (var size in sizes)
    //    {
    //        if (xPos + size.Width > bounds.X + bounds.Width)
    //        {
    //            rows.Add(currentRow);
    //            heights.Add(currentRowHeight);

    //            currentRow = new List<Rectangle>();
    //            xPos = bounds.X;
    //            currentRowHeight = 0;
    //        }

    //        currentRow.Add(size);
    //        currentRowHeight = (int)Math.Max(currentRowHeight, size.Height);
    //        xPos += size.Width + 10;
    //    }

    //    if (currentRow.Count > 0)
    //    {
    //        rows.Add(currentRow);
    //        heights.Add(currentRowHeight);
    //    }

    //    return (rows, heights);
    //}

    public virtual new Dialog AddContent(UIContent content)
    {
        return (Dialog)base.AddContent(content);
    }

    public new Dialog AddButton(RichText text, ButtonClickHandler? onClick = null)
    {
        ButtonRowContent? btns = null;
        foreach (var item in Contents)
        {
            if (item is ButtonRowContent b)
            {
                btns = b;
                break;
            }
        }

        if (btns is null)
        {
            btns = new();
            AddContent(btns);
        }

        btns.AddButton(text, onClick);
        return this;
    }

    /// <summary>
    /// Adds a button to the toast
    /// </summary>
    /// <param name="text"></param>
    /// <param name="onClick">Should return true when the toast should close, false if not</param>
    /// <returns></returns>
    public new Dialog AddButton(string text, ButtonClickHandler? onClick) => AddButton(RichText.Parse(text, Color.White), onClick);

    /// <summary>
    /// Adds a progress bar to the toast
    /// </summary>
    /// <param name="initialProgress">The progress in a 0-1 range. set to -1 to have it do a infinite working animation</param>
    /// <param name="ProgressProvider">The function that provides further values</param>
    /// <param name="closesToastWhenComplete">When true, and the progress becomes 1, it requests the toast to close.</param>
    /// <returns></returns>
    public new Dialog AddProgressBar(float initialProgress, Func<float, float>? ProgressProvider = null, bool closesToastWhenComplete = true, string infiniteSpinText = "Working...")
    {
        return AddContent(new UIProgressContent(initialProgress, ProgressProvider, closesToastWhenComplete, infiniteSpinText));
    }

    /// <summary>
    /// Adds the sprite to the dialog
    /// </summary>
    /// <param name="sprite"></param>
    /// <returns></returns>
    public new Dialog AddSprite(Sprite sprite) => AddContent(new UISpriteContent(sprite));

    public new Dialog AddTitle(string text, UIFontSizePreset preset = UIFontSizePreset.Title)
    => AddText(RichText.Parse(text, Color.White), preset);
    public new Dialog AddTitle(RichText text, UIFontSizePreset preset = UIFontSizePreset.Title)
        => AddContent(new UIMessageContent(text, preset));
    public new Dialog AddText(RichText text, UIFontSizePreset preset = UIFontSizePreset.Message)
        => AddContent(new UIMessageContent(text, preset));

    public new Dialog AddText(string text, UIFontSizePreset preset = UIFontSizePreset.Message)
        => AddText(RichText.Parse(text, Color.White), preset);
}

//public abstract class Dialog
//{
//    List<Rectangle> buttonSizes = [];

//    public RichText Title { get; set; }
//    public RichText Message { get; set; }
//    public DialogPlacement Placement { get; set; }
//    public DialogPriority Priority { get; }

//    public Rectangle Bounds => Dialogs.GetDialogBounds(Placement);

//    public bool IsClosing { get; internal set; }

//    public Action<UIContext>? OnImGui { get; set; }

//    private const int spacing = 10;
//    private const int paddingX = 12;
//    private const int paddingY = 6;

//    public List<DialogButton> Buttons { get; }

//    public bool IsVisible => !IsClosing;

//    public DialogStyle Style { get; set; } = new();
//    public float YAnimateTime { get; internal set; }
//    internal bool WasBumped { get; set; }
//    public DialogAnimation CurrentAnim { get; internal set; } = new() { Elapsed = 0f };

//    public InputContext Input => Dialogs.Input;

//    /// <summary>
//    /// Used for Toast to dialog morph
//    /// </summary>
//    internal bool DrawContentOnly { get; set; }
//    /// <summary>
//    /// Used for Toast to dialog morph
//    /// </summary>
//    internal Rectangle LastScaledBoudningBox { get; set; }
//    /// <summary>
//    /// Used for Toast to dialog morph
//    /// </summary>
//    internal float lastInnerWidth { get; set; }

//    protected Dialog(
//        string title,
//        string message,
//        DialogPlacement placement,
//        DialogPriority priority,
//        string[]? buttons,
//        Func<bool>[]? onButtonClick,
//        Action<UIContext>? onImGui)
//    {
//        Rectangle bounds = Dialogs.GetDialogBounds(Placement);
//        float scaleRef = Math.Min(bounds.Width, bounds.Height);
//        float titleScale = scaleRef * 0.09f;
//        float messageScale = scaleRef * 0.04f;

//        Title = RichText.Parse(title, Color.White);
//        Title.FontSize = (int)Math.Clamp(titleScale, 14, 36);

//        Message = RichText.Parse(message, Color.White);
//        Message.FontSize = (int)Math.Clamp(messageScale, 10, 24);


//        Placement = placement;
//        Priority = priority;

//        buttons ??= [];
//        onButtonClick ??= [];

//        Buttons = new List<DialogButton>(buttons.Length);

//        for (int i = 0; i < buttons.Length; i++)
//        {
//            string label = buttons[i];
//            Func<bool>? onClick = null;
//            if (onButtonClick.Length < i)
//                onClick = onButtonClick[i]!;

//            Buttons.Add(new DialogButton(label, onClick));
//        }

//        IsClosing = false;
//        OnImGui = onImGui;
//    }

//    public virtual void Close()
//    {
//        IsClosing = true;
//        CurrentAnim = CurrentAnim with
//        {
//            Elapsed = 0,
//            Completed = false
//        };
//    }

//    internal void RenderBox(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth, ref int y)
//    {
//        float baseFontScale = (bounds.Height / 600f + bounds.Width / 800f) / 2f;

//        RichTextRenderer.DrawRichText(Title, new((int)bounds.X + padding, y), innerWidth, Style.ContentColor, Input);
//        y += 35 + (int)Title.CalculateBounds(innerWidth).Height;

//        RichTextRenderer.DrawRichText(Message, new((int)bounds.X + padding, y), innerWidth, Style.ContentColor, Input);
//        y += 40 + (int)Message.CalculateBounds(innerWidth).Height;

//        DrawContent(bounds, contentAlpha, ref padding, ref innerWidth, ref y);

//        // --- Buttons ---
//        buttonSizes.Clear();

//        // scale button font size similarly
//        int buttonBaseSize = 12;
//        int buttonFontSize = (int)(buttonBaseSize * baseFontScale);
//        buttonFontSize = Math.Clamp(buttonFontSize, 12, 28); // keep sane range

//        for (int i = 0; i < Buttons.Count; i++)
//        {
//            Buttons[i].text.FontSize = buttonFontSize; // apply scale

//            Rectangle textSize = Buttons[i].text.CalculateBounds(innerWidth);
//            int btnWidth = (int)textSize.Width + paddingX * 2;
//            int btnHeight = (int)textSize.Height + paddingY * 2;
//            buttonSizes.Add(new Rectangle(0, 0, btnWidth, btnHeight));
//        }

//        // Collect rows
//        List<List<Rectangle>> rows = new();
//        List<int> rowHeights = new();
//        float xPos = bounds.X;
//        int currentRowHeight = 0;
//        List<Rectangle> currentRow = new();

//        for (int i = 0; i < Buttons.Count; i++)
//        {
//            Rectangle size = buttonSizes[i];

//            if (xPos + size.Width > bounds.X + bounds.Width)
//            {
//                rows.Add(currentRow);
//                rowHeights.Add(currentRowHeight);

//                currentRow = new List<Rectangle>();
//                xPos = bounds.X;
//                y += currentRowHeight + spacing;
//                currentRowHeight = 0;
//            }

//            currentRow.Add(size);
//            currentRowHeight = (int)Math.Max(currentRowHeight, size.Height);
//            xPos += size.Width + spacing;
//        }

//        if (currentRow.Count > 0)
//        {
//            rows.Add(currentRow);
//            rowHeights.Add(currentRowHeight);
//        }

//        // Calculate total button block height
//        int totalButtonHeight = rowHeights.Sum() + spacing * (rowHeights.Count - 1);

//        // If buttons overflow dialog bottom, shift them up
//        int buttonsY = y;
//        if (buttonsY + totalButtonHeight > bounds.Y + bounds.Height)
//        {
//            buttonsY = (int)bounds.Y + (int)bounds.Height - totalButtonHeight;
//        }

//        // Draw rows
//        float rowY = buttonsY;
//        int buttonIndex = 0;
//        for (int r = 0; r < rows.Count; r++)
//        {
//            float rowWidth = rows[r].Sum(b => b.Width) + spacing * (rows[r].Count - 1);
//            float rowX = bounds.X + (bounds.Width - rowWidth) / 2; // center row

//            for (int b = 0; b < rows[r].Count; b++)
//            {
//                Rectangle btnRect = new((int)rowX, (int)rowY, rows[r][b].Width, rows[r][b].Height);
//                Buttons[buttonIndex++].Draw(this, Style, btnRect);
//                rowX += rows[r][b].Width + spacing;
//            }

//            rowY += rowHeights[r] + spacing;
//        }

//        y = (int)rowY;

//        // --- ImGui / Additional UI ---
//        UIContext c = new UIContext();
//        c.Begin(new Vector2(bounds.X, y), Style.ContentColor);
//        OnImGui?.Invoke(c);
//        c.End();
//    }


//    internal void UpdateBox()
//    {
//        Update();
//    }

//    public abstract void DrawContent(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth, ref int y);
//    public abstract void Update();
//}