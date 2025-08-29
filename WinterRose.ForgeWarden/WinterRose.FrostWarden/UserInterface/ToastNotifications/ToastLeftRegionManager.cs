using Raylib_cs;
using WinterRose.ForgeWarden.UserInterface;

namespace WinterRose.ForgeWarden.UserInterface.ToastNotifications;

public class ToastLeftRegionManager() : ToastRegionManager(ToastRegion.Left)
{
    protected override Vector2 GetEntryPosition(ToastStackSide side, Toast toast, float y)
    {
        return new Vector2(
            UIConstants.TOAST_SPACING,
            y);
    }

    protected override Rectangle GetInitialDialogPosition(ToastStackSide side, Toast toast, float y)
    {
        return new(-Toasts.TOAST_WIDTH, y, Toasts.TOAST_WIDTH, toast.Height);
    }

    protected override float GetToastXPosition(Toast toast) => UIConstants.TOAST_SPACING;

    protected internal override Rectangle GetExitPositionAndScale(Toast toast) =>
        new(-toast.CurrentPosition.Width,
            toast.CurrentPosition.Y,
            toast.CurrentPosition.Width,
            toast.CurrentPosition.Height);
}