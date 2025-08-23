using Raylib_cs;
using System.Buffers;
using System.ComponentModel;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;

namespace WinterRose.ForgeWarden.UserInterface.ToastNotifications;

public class Toast : UIContainer
{
    public ToastType Type;

    public override InputContext Input => Toasts.Input;

    protected internal ToastRegionManager ToastManager { get; internal set; }

    private float raiseElapsed = 0f;
    private bool isHoverTarget = false;
    private Func<Toast, int> continueWithSelector;
    private Toast[] continueWithOptions;

    public ToastStackSide StackSide { get; internal set; }
    public ToastRegion Region { get; internal set; }

    public override float Height => Contents.Sum(c => c.GetHeight(Toasts.TOAST_WIDTH)) + base.Height;

    public Toast(ToastType type, ToastRegion region = ToastRegion.Right, ToastStackSide stackSide = ToastStackSide.Bottom) : this()
    {
        Type = type;
        StackSide = stackSide;
        Style = new ToastStyle(type);
        Region = region;
    }

    public Toast(ToastType type, ToastStyle style, ToastRegion region = ToastRegion.Right, ToastStackSide stackSide = ToastStackSide.Bottom) : this()
    {
        Type = type;
        StackSide = stackSide;
        Style = style;
        Region = region;
    }

    private Toast()
    {
        TimeUntilAutoDismiss = 5;
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Close()
    {
        if (!IsClosing)
        {
            if (ToastManager != null)
            {
                Rectangle exitTarget = ToastManager.GetExitPositionAndScale(this);
                TargetPosition = exitTarget.Position;
                TargetScale = exitTarget.Size;
            }
            else
                TargetScale = new();
        }

        base.Close();
    }

    protected override void AlterBoundsCorrectlyForDragBar(ref Rectangle backgroundBounds, float dragHeight)
    {
        if (StackSide == ToastStackSide.Top)
            backgroundBounds.Height += dragHeight;
        else
        {
            backgroundBounds.Y -= dragHeight / 2;
            backgroundBounds.Height += dragHeight / 2;
        }
    }

    public virtual new Toast AddContent(UIContent content)
    {
        return (Toast)base.AddContent(content);
    }

    public new Toast AddButton(RichText text, ButtonClickHandler? onClick = null)
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
    public new Toast AddButton(string text, ButtonClickHandler? onClick) => AddButton(RichText.Parse(text, Color.White), onClick);

    /// <summary>
    /// Adds a progress bar to the toast
    /// </summary>
    /// <param name="initialProgress">The progress in a 0-1 range. set to -1 to have it do a infinite working animation</param>
    /// <param name="ProgressProvider">The function that provides further values</param>
    /// <param name="closesToastWhenComplete">When true, and the progress becomes 1, it requests the toast to close.</param>
    /// <returns></returns>
    public new Toast AddProgressBar(float initialProgress, Func<float, float>? ProgressProvider = null, bool closesToastWhenComplete = true, string infiniteSpinText = "Working...")
    {
        return AddContent(new UIProgressContent(initialProgress, ProgressProvider, closesToastWhenComplete, infiniteSpinText));
    }

    /// <summary>
    /// Adds the sprite to the dialog
    /// </summary>
    /// <param name="sprite"></param>
    /// <returns></returns>
    public new Toast AddSprite(Sprite sprite) => AddContent(new UISpriteContent(sprite));

    public new Toast AddTitle(string text, UIFontSizePreset preset = UIFontSizePreset.Title)
    => AddText(RichText.Parse(text, Color.White), preset);
    public new Toast AddTitle(RichText text, UIFontSizePreset preset = UIFontSizePreset.Title)
        => AddContent(new UIMessageContent(text, preset));
    public new Toast AddText(RichText text, UIFontSizePreset preset = UIFontSizePreset.Message)
        => AddContent(new UIMessageContent(text, preset));

    public new Toast AddText(string text, UIFontSizePreset preset = UIFontSizePreset.Message)
        => AddText(RichText.Parse(text, Color.White), preset);

    /// <summary>
    /// Closes this Toast as a toast, and opens it as a dialog with a morphin motion
    /// </summary>
    /// <param name="targetDialog"></param>
    /// <param name="morphDuration"></param>
    /// <param name="toastFadeStart"></param>
    /// <param name="toastFadeEnd"></param>
    /// <param name="dialogFadeStart"></param>
    public void OpenAsDialog(Dialog targetDialog, float morphDuration = 0.5f, float toastFadeStart = 0.05f, float toastFadeEnd = 0.4f, float dialogFadeStart = 0.55f)
    {
        ToastToDialogMorpher.TryStartMorph(this, targetDialog, morphDuration, toastFadeStart, toastFadeEnd, dialogFadeStart);
    }

    /// <summary>
    /// Call this to have one and only one continuation toast
    /// </summary>
    /// <param name="toast"></param>
    public void ContinueWith(Toast toast)
    {
        ContinueWith(t => 0, toast);
    }

    /// <summary>
    /// Call this to have multiple continuations based based on some condition evaluated when the continuation should happen
    /// </summary>
    /// <param name="continueWithSelector"></param>
    /// <param name="options"></param>
    public void ContinueWith(Func<Toast, int> continueWithSelector, params Toast[] options)
    {
        this.continueWithSelector = continueWithSelector;
        continueWithOptions = options;
    }

    internal Toast? GetContinueWithToast()
    {
        if (continueWithSelector == null || continueWithOptions == null || continueWithOptions.Length == 0)
            return null;

        int index;
        try
        {
            index = continueWithSelector(this);
        }
        catch
        {
            return null;
        }

        if (index < 0 || index >= continueWithOptions.Length)
            return null;

        return continueWithOptions[index];
    }
}



