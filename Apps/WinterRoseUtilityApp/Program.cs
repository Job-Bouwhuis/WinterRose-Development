using BulletSharp.SoftBody;
using PuppeteerSharp;
using Raylib_cs;
using System.Numerics;
using WinterRose;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.Geometry;
using WinterRose.ForgeWarden.Geometry.Animation;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Tooltipping;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.ForgeWarden.Utility;
using WinterRose.ForgeWarden.Worlds;
using WinterRose.Recordium;
using WinterRose.WinterForgeSerializing;
using WinterRose.WIP.TestClasses;
using WinterRoseUtilityApp.SubSystems;
using dialog = WinterRose.ForgeWarden.UserInterface.DialogBoxes.Dialog;

namespace WinterRoseUtilityApp;

internal class Program() : ForgeWardenEngine(GracefulErrorHandling: false)
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

    // Comprehensive demo of all new RichText features with modifier lifetime system
    public static string GetRichTextDemo()
    {
        return @"\color[gold]\bold[]=== RICH TEXT FEATURE DEMO ===\end[bold]\color[white]

\color[cyan]\bold[]Text Styling:\end[bold]\color[white]
  • \bold[]Bold Text\end[bold] - Uses multi-pass stroke rendering
  • \italic[]Italic Text\end[italic] - Character-by-character skew effect
  • \bold[]\color[yellow]Bold + Yellow\color[white]\end[bold]

\color[cyan]\bold[]Animated Effects:\end[bold]\color[white]
  • \wave[amplitude=4;speed=2.5;wavelength=1]Waving Text!\end[wave]
  • \shake[intensity=3;speed=12]Shaking Alert!\end[shake]
  

\color[cyan]\bold[]Interactive Elements:\end[bold]\color[white]
  • Progress: \progress[value=65;max=100;width=150]
  • Status: \progress[value=0.8;width=120]
  • \tt[Hover over me|This is a tooltip!] for more info

\color[cyan]\bold[]Composition Examples:\end[bold]\color[white]
  • \bold[]\color[red]ERROR\color[white]\end[bold]: \shake[intensity=2]Critical system failure\end[shake]
  • \bold[]\color[green]SUCCESS\color[white]\end[bold]: \wave[]Operation complete!\end[wave]
  • Loading: \progress[value=42;max=100;width=100]

\color[cyan]\bold[]Nested Modifiers:\end[bold]\color[white]
  • \bold[]\italic[]Bold AND italic\end[italic]\end[bold] text
  • \shake[]\wave[amplitude=3]Both animations together\end[wave]\end[shake]
  • Visit \link[https://github.com]our \bold[]GitHub\end[bold]\end[link]

\color[cyan]\bold[]Mixed Styling:\end[bold]\color[white]
  • Press \bold[\color[yellow]F1\color[white]\end[bold] for \tt[help|\wave[]Press F1 to open the help menu \progress[value=0.5]]
  • Status: \italic[\color[yellow]Awaiting user input...\end[italic]\color[white]
  • \link[https://example.com]\color[cyan]Clickable example\color[white]\end[link]

\color[gold]\bold[]=== End of Demo ===\end[bold]";
    }

    public override World CreateFirstWorld()
    {
        LogDestinations.AddDestination(logConsole = new InAppLogConsole());
        Raylib.SetTargetFPS(144);

        RichTextRenderer.FunctionRegistry.RegisterFunction(new FunctionDefinition("test", FunctionResult (string functionName,
            Dictionary<string, string> arguments,
            RichTextRenderContext context,
            Vector2 position) => {

                Toasts.Neutral($"Function '{functionName}' called with arguments: " +
                    $"{string.Join(", ", arguments.Select(kv => $"{kv.Key}={kv.Value}"))}");

                return new FunctionResult();
        }));

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

        t.AddContent(new UIButton("File Browser", (c, b) =>
        {
            UIWindow wind = new UIWindow("File Browser", 500, 600);
            wind.AddFileExplorer();
            wind.ShowMaximized();
        }));

        t.AddContent(new UIButton("Rich Text Test window", (c, b) =>
        {
            UIWindow test = new UIWindow("RichText Demo", 400, 300);
            test.AddText(GetRichTextDemo());

            test.AddContent(new UISpacer());

            test.AddText("lets make an inline \\btn[button;test;arg1=5] cool no?\n" +
                "this is like genuinely really darn cool like i wouldnt want it any other way");

            test.Show();
        }));

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