using Raylib_cs;
using System.ComponentModel;

namespace WinterRose.ForgeWarden.UserInterface.ToastNotifications;

public class ToastCenterRegionManager : ToastRegionManager
{
    public ToastCenterRegionManager() : base(ToastRegion.Center) { }

    protected override Vector2 GetEntryPosition(ToastStackSide side, Toast toast, float y) 
        => new Vector2(
            (Application.Current.Window.Width - Toasts.TOAST_WIDTH) / 2f,
            y
        );

    protected override Rectangle GetInitialDialogPosition(ToastStackSide side, Toast toast, float y) 
        => new Rectangle(
            (Application.Current.Window.Width - Toasts.TOAST_WIDTH) / 2f,
            side switch
            {
                ToastStackSide.Top => -toast.Height,
                ToastStackSide.Bottom => Application.Current.Window.Height + toast.Height,
                _ => throw new InvalidEnumArgumentException(nameof(side), (int)side, typeof(ToastStackSide)),
            },
            Toasts.TOAST_WIDTH,
            toast.Height
        );

    protected override float GetToastXPosition(Toast toast) =>
        (Application.Current.Window.Width - Toasts.TOAST_WIDTH) / 2f;


    protected internal override Rectangle GetExitPositionAndScale(Toast toast) =>
        new Rectangle(
            toast.CurrentPosition.X,
            toast.CurrentPosition.Y,
            0,
            0
        );
}

