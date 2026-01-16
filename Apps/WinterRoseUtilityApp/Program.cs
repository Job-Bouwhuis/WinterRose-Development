using BulletSharp.SoftBody;
using PuppeteerSharp;
using Raylib_cs;
using System.Numerics;
using WinterRose;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.Geometry;
using WinterRose.ForgeWarden.Geometry.Animation;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.ForgeWarden.Utility;
using WinterRose.ForgeWarden.Worlds;
using WinterRose.Recordium;
using WinterRose.WinterForgeSerializing;
using WinterRoseUtilityApp.SubSystems;
using dialog = WinterRose.ForgeWarden.UserInterface.DialogBoxes.Dialog;

namespace WinterRoseUtilityApp;

internal class Program() : ForgeWardenEngine(GracefulErrorHandling: true)
{
    public static new Program Current => (Program)ForgeWardenEngine.Current;
    SubSystemManager subSystemManager;
    Windows.SystemTrayIcon trayIcon;
    private List<UIContent> trayItems = [];
    private InAppLogConsole logConsole;
    private bool faulted = false;
    private const bool forceWindow = false;

    private static async Task Main(string[] args)
    {
        Dictionary<Dictionary<List<int>, List<string>>, List<bool>> a = [];

        if (OperatingSystem.IsLinux() || forceWindow)
            new Program().Run("WinterRose Util App", 1280, 720);
        else
            new Program().RunAsOverlay(monitorIndex: 1);
    }

    public override World CreateFirstWorld()
    {
        LogDestinations.AddDestination(logConsole = new InAppLogConsole());
        Raylib.SetTargetFPS(144);
        subSystemManager = new SubSystemManager();
        subSystemManager.Initialize().ContinueWith(t =>
        {
            if (t.IsFaulted)
                Environment.Exit(37707);
        });

        GlobalHotkey.RegisterHotkey("OpenLogConsole", true, HotkeyScancode.LeftAlt, HotkeyScancode.L);
        GlobalHotkey.RegisterHotkey("OpenTray", true, HotkeyScancode.LeftAlt, HotkeyScancode.Q);

        if (OperatingSystem.IsWindows())
        {
            trayIcon = new Windows.SystemTrayIcon(Window.Handle, 0, "WinterRose Utils", "AppLogo.ico");
            trayIcon.ShowInTray();
            trayIcon.RightClick.Subscribe(Invocation.Create(CreateTray));
        }
        return new World("");
    }

    private void CreateTray()
    {
        Toast t = new Toast(ToastType.Neutral, ToastRegion.Right, ToastStackSide.Bottom);
        t.Style.TimeUntilAutoDismiss = 8;

        foreach (var item in trayItems)
            t.AddContent(item);

        bool currentStartupState = StartupManager.IsStartupEnabled();
        UICheckBox startup = new UICheckBox("Start when the PC does?",
            Invocation.Create((IUIContainer c, UICheckBox b, bool newState) =>
            {
                b.IndicateBusy = true;
                newState = !StartupManager.IsStartupEnabled();
                StartupManager.SetStartup(newState).ContinueWith(t =>
                {
                    b.ForceSetChecked(StartupManager.IsStartupEnabled(), true);
                    if (b.Checked != newState)
                        Toasts.Error("Failed marking app to start with the OS");
                    else if (b.Checked)
                        Toasts.Success("The app will now start when your OS starts!");
                    else
                        Toasts.Success("The app will no longer start when your OS starts!");

                    b.IndicateBusy = false;
                });

            }
        ), currentStartupState);

        t.AddContent(startup);
        t.AddButton("Close App",
            Invocation.Create<IUIContainer, UIButton>((c, b) => Close()));
        Toasts.ShowToast(t);
    }

    public override void Closing()
    {
        trayIcon?.DeleteIcon();
    }

    public override void Update()
    {
        if (faulted || !subSystemManager.Initialized)
            return;
        if (GlobalHotkey.IsTriggered("OpenLogConsole"))
            logConsole.Show();
        if (GlobalHotkey.IsTriggered("OpenTray"))
            CreateTray();

        subSystemManager.Tick();
    }

    internal void AddTrayItem(UIContent uIButton)
    {
        trayItems.Add(uIButton);
    }
}