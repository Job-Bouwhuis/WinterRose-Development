using BulletSharp.SoftBody;
using Microsoft.Graph.IdentityGovernance.AccessReviews.Definitions.FilterByCurrentUserWithOn;
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
    public static new Program Current => (Program)Application.Current;
    SubSystemManager subSystemManager;
    Windows.SystemTrayIcon trayIcon;
    private List<UIContent> trayItems = [];

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

            foreach (var item in trayItems)
                t.AddContent(item);

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

    internal void AddTrayItem(UIContent uIButton)
    {
        trayItems.Add(uIButton);
    }
}