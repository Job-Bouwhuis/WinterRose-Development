using Raylib_cs;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.Recordium;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace WinterRose.ForgeWarden.UserInterface.DialogBoxes;

public class Dialog : UIContainer
{
    private static Log log = new Log("Dialogs");

    public DialogPlacement Placement { get; set; }
    public DialogPriority Priority { get; }

    public Rectangle DialogPlacementBounds => Dialogs.GetDialogBounds(Placement);

    public Action<Dialog, object>? OnResult { get; set; }
    /// <summary>
    /// If the dialog has not yet closed, this value may still change!
    /// </summary>
    public object? DialogResult { get; set; }

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

        NoAutoMove = true;

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

        UIText titleContent = new UIText(title, UIFontSizePreset.Title);
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

        UIText messageContent = new UIText(message, UIFontSizePreset.Text);
        messageContent.Text.FontSize = (int)Math.Clamp(messageScale, 10, 24);

        messageContent.owner = this;
        Contents.Insert(1, messageContent);
    }

    public override void Close()
    {
        base.Close();

        CurrentAnim = CurrentAnim with { Elapsed = 0, Completed = false };

        if (OnResult != null)
        {
            OnResult.Invoke(this, DialogResult ?? new object());
        }
    }

    public virtual new Dialog AddContent(UIContent content)
    {
        return (Dialog)base.AddContent(content);
    }

    public virtual Dialog AddButton(RichText text, Action<UIContainer, UIButton> onClick)
        => AddButton(text, Invocation.Create(onClick));

    public new Dialog AddButton(RichText text, VoidInvocation<UIContainer, UIButton>? onClick = null)
    {
        AddContent(new UIButton(text, onClick));
        return this;
    }

    /// <summary>
    /// Adds a button to the toast
    /// </summary>
    /// <param name="text"></param>
    /// <param name="onClick">Should return true when the toast should close, false if not</param>
    /// <returns></returns>
    public new Dialog AddButton(string text, VoidInvocation<UIContainer, UIButton>? onClick) => AddButton(RichText.Parse(text, Color.White), onClick);

    /// <summary>
    /// Adds a progress bar to the toast
    /// </summary>
    /// <param name="initialProgress">The progress in a 0-1 range. set to -1 to have it do a infinite working animation</param>
    /// <param name="ProgressProvider">The function that provides further values</param>
    /// <param name="closesToastWhenComplete">When true, and the progress becomes 1, it requests the toast to close.</param>
    /// <returns></returns>
    public new Dialog AddProgressBar(float initialProgress, Func<float, float>? ProgressProvider = null, string infiniteSpinText = "Working...")
    {
        return AddContent(new UIProgress(initialProgress, ProgressProvider, infiniteSpinText));
    }

    /// <summary>
    /// Adds the sprite to the dialog
    /// </summary>
    /// <param name="sprite"></param>
    /// <returns></returns>
    public new Dialog AddSprite(Sprite sprite) => AddContent(new UISprite(sprite));

    public new Dialog AddTitle(string text, UIFontSizePreset preset = UIFontSizePreset.Title)
    => AddText(RichText.Parse(text, Color.White), preset);
    public new Dialog AddTitle(RichText text, UIFontSizePreset preset = UIFontSizePreset.Title)
        => AddContent(new UIText(text, preset));
    public new Dialog AddText(RichText text, UIFontSizePreset preset = UIFontSizePreset.Text)
        => AddContent(new UIText(text, preset));

    public new Dialog AddText(string text, UIFontSizePreset preset = UIFontSizePreset.Text)
        => AddText(RichText.Parse(text, Color.White), preset);

    public void Show() => Dialogs.Show(this);
}