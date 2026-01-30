using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.EventBusses;

using btnPair = (string, System.Action);
using btnPair1 = (string, System.Action<WinterRose.ForgeWarden.UserInterface.IUIContainer, WinterRose.ForgeWarden.UserInterface.UIButton>);

namespace WinterRose.ForgeWarden.UserInterface.DialogBoxes;

public class ConfirmationDialog : Dialog
{
    public enum Preset
    {
        Ok,
        YesNo,
        RetryAbort
    }

    public ConfirmationDialog(string message,
        string title = "Alert",
        DialogPlacement placement = DialogPlacement.CenterSmall,
        DialogPriority priority = DialogPriority.Normal)
        : base(title, message, placement, priority)
    {
    }
    public ConfirmationDialog(string message,
        string title = "Alert",
        params (string btn, Action<IUIContainer, UIButton> OnClick)[] buttons)
        : base(title, message, DialogPlacement.CenterSmall, DialogPriority.Normal)
    {
        ConstructButtons(buttons);
    }

    private void ConstructButtons(params btnPair1[] buttons)
    {
        UIColumns cols = new UIColumns();
        cols.ColumnCount = buttons.Length;

        for (int i = 0; i < buttons.Length; i++)
            cols.AddToColumn(i, new UIButton(buttons[i].Item1, Invocation.Create(buttons[i].Item2)));
        AddContent(cols);
    }

    public ConfirmationDialog(
        string message,
        string title = "Alert",
        params (string btn, Action OnClick)[] buttons)
        : base(title, message, DialogPlacement.CenterSmall, DialogPriority.Normal)
    {
        
        ConstructButtons(buttons);
    }

    private void ConstructButtons(params btnPair[] buttons)
    {
        UIColumns cols = new UIColumns();
        cols.ColumnCount = buttons.Length;
        for (int i = 0; i < buttons.Length; i++)
            cols.AddToColumn(i, new UIButton(buttons[i].Item1, (c, b) => buttons[i].Item2()));
        AddContent(cols);
    }

    public ConfirmationDialog(string message, Preset preset, Action<string>? OnConfirm = null, string title = "Alert")
        : base("Alert", message, DialogPlacement.CenterSmall, DialogPriority.Normal)
    {
        Action<IUIContainer, UIButton> action = (c, b) =>
        {
            OnConfirm?.Invoke(b.Text.ToString());
            c.Close();
        };

        switch (preset)
        {
            case Preset.Ok:
                ConstructButtons(("Ok", action));
                break;
            case Preset.YesNo:
                ConstructButtons(("Yes", action), ("No", action));
                break;
            case Preset.RetryAbort:
                ConstructButtons(("Retry", action), ("Abort", action));
                break;
        }
    }

    public ConfirmationDialog(string message, Preset preset, Func<string, bool>? OnConfirm = null, string title = "Alert")
    : base("Alert", message, DialogPlacement.CenterSmall, DialogPriority.Normal)
    {
        Action<IUIContainer, UIButton> action = (c, b) =>
        {
            var res = OnConfirm?.Invoke(b.Text.ToString()) ?? true;
            if (res)
                c.Close();
        };

        switch (preset)
        {
            case Preset.Ok:
                ConstructButtons(("Ok", action));
                break;
            case Preset.YesNo:
                ConstructButtons(("Yes", action), ("No", action));
                break;
            case Preset.RetryAbort:
                ConstructButtons(("Retry", action), ("Abort", action));
                break;
        }
    }
}
