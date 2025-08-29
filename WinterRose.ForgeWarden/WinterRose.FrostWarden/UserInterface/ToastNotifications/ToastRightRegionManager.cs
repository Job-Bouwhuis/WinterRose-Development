using Raylib_cs;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.UserInterface;

namespace WinterRose.ForgeWarden.UserInterface.ToastNotifications;

public class ToastRightRegionManager() : ToastRegionManager(ToastRegion.Right)
{
    protected override Vector2 GetEntryPosition(ToastStackSide side, Toast toast, float y)
    {
        return new Vector2(
            Application.Current.Window.Width - Toasts.TOAST_WIDTH - UIConstants.TOAST_SPACING, 
            y);
    }

    protected override Rectangle GetInitialDialogPosition(ToastStackSide side, Toast toast, float y)
    {
        return new(Application.Current.Window.Width, y, Toasts.TOAST_WIDTH, toast.Height);
    }

    protected override float GetToastXPosition(Toast toast) =>
        Application.Current.Window.Width - Toasts.TOAST_WIDTH - UIConstants.TOAST_SPACING;

    protected internal override Rectangle GetExitPositionAndScale(Toast toast) =>
        new(Application.Current.Window.Width,
            toast.CurrentPosition.Y,
            toast.CurrentPosition.Width,
            toast.CurrentPosition.Height);
}
