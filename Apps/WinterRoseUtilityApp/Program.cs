using BulletSharp.SoftBody;
using WinterRose;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.Worlds;
using WinterRoseUtilityApp.SubSystems;

namespace WinterRoseUtilityApp;

internal class Program : Application
{
    SubSystemManager subSystemManager;
    Windows.SystemTrayIcon trayIcon;
    private static void Main(string[] args)
    {
        new Program().RunAsOverlay();
        
    }

    public override World CreateFirstWorld()
    {
        Raylib_cs.Raylib.SetTargetFPS(144);
        subSystemManager = new SubSystemManager();
        if (!subSystemManager.Initialize())
            Close();

        trayIcon = new Windows.SystemTrayIcon(Window.Handle, 0, "WinterRose Utils", "AppLogo.ico");
        trayIcon.ShowInTray();
        trayIcon.RightClick.Subscribe(Invocation.Create(() =>
        {
            Toast t = new Toast(ToastType.Neutral, ToastRegion.Right, ToastStackSide.Bottom);
            t.Style.TimeUntilAutoDismiss = 8;
            t.AddButton("Close App", 
                Invocation.Create<UIContainer, UIButton>((c, b) => Close()));
            Toasts.ShowToast(t);
        }));
        return new World("");
    }

    public override void Closing()
    {
        trayIcon.DeleteIcon();
    }

    public override void Update()
    {
        GlobalHotkey.Update();
        subSystemManager.Tick();
    }

    
    
}