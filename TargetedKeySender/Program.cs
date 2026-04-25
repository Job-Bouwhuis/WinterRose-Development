using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TargetedKeySender;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.ForgeWarden.Worlds;
using static WinterRose.Windows;

internal class Program() : ForgeWardenEngine(UseBrowser: false, fancyShutdown: false)
{
    KeyAutomation automation = new KeyAutomation();
    TargetWindow selectedWindow = null;

    SystemTrayIcon trayIcon;
    private static void Main(string[] args)
    {
        new Program().RunAsOverlay();
    }

    public override void AfterWindowCreation()
    {
        unsafe
        {
            trayIcon = new SystemTrayIcon((nint)Raylib.GetWindowHandle(), 0, "Targeted Key Sender", "appico.ico");
            trayIcon.ShowInTray();
             trayIcon.Click.Subscribe(() =>
             {
                CreateAutomationWindow().Show();
             });
             trayIcon.RightClick.Subscribe(() =>
             {
                 trayIcon.DeleteIcon();
                 Close();
             });
        }
    }

    public override World CreateFirstWorld() => new("");

    public UIWindow CreateAutomationWindow()
    {
        UIWindow window = new UIWindow("Key Automation", 700, 500);

        ushort selectedKey = KeyCodes.OEM_4;
        int interval = 3000;

        Action rebuild = null!;

        rebuild = () =>
        {
            window.ClearContent();

            // =========================
            // ACTIVE AUTOMATION VIEW
            // =========================
            if (automation.IsRunning)
            {
                window.AddContent(new UIText("Automation Running", UIFontSizePreset.Title));
                window.AddContent(new UIText($"Target: {selectedWindow?.Title}"));
                window.AddContent(new UIText($"Key: {selectedKey}"));
                window.AddContent(new UIText($"Interval: {interval} ms"));

                window.AddContent(new UISpacer());

                window.AddContent(new UIButton("Stop", (c, b) =>
                {
                    automation.Stop();
                    rebuild();
                }));

                return;
            }

            // =========================
            // APP SELECTION VIEW
            // =========================

            window.AddContent(new UIText("Select Application", UIFontSizePreset.Title));

            List<TargetWindow> windows = WindowScanner.GetWindows();

            if (windows.Count == 0)
            {
                window.AddContent(new UIText("No applications found"));
                return;
            }

            foreach (var w in windows)
            {
                UIButton btn = new UIButton($"{w.ProcessName} - {w.Title}", (c, b) =>
                {
                    selectedWindow = w;
                    rebuild();
                });

                window.AddContent(btn);
            }

            // =========================
            // CONFIG VIEW
            // =========================

            if (selectedWindow != null)
            {
                window.AddContent(new UISpacer());
                window.AddContent(new UIText("Configure Automation", UIFontSizePreset.Subtitle));
                window.AddContent(new UIText($"Selected: {selectedWindow.Title}"));

                // KEY INPUT
                UITextInput keyInput = new UITextInput()
                {
                    Placeholder = "Enter key (e.g. [ )"
                };

                keyInput.OnInputChanged.Subscribe(Invocation.Create<UITextInput, string>((i, text) =>
                {
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        char c = text[0];
                        selectedKey = (ushort)char.ToUpper(c);
                    }
                }));

                window.AddContent(keyInput);

                // INTERVAL SLIDER
                UIValueSlider<int> slider = new UIValueSlider<int>
                {
                    Label = "Interval (ms)",
                    MinValue = 2000,
                    MaxValue = 10000,
                    Step = 500,
                    SnapToStep = true
                };

                slider.SetValue(interval, false);

                slider.OnValueChangedBasic.Subscribe(Invocation.Create<int>(v =>
                {
                    interval = v;
                }));

                window.AddContent(slider);

                // START BUTTON
                window.AddContent(new UIButton("Start Automation", async (c, b) =>
                {
                    if (selectedWindow == null)
                    {
                        Toasts.Error("No window selected!");
                        return;
                    }

                    _ = automation.Start(selectedWindow, selectedKey, interval);
                    rebuild();
                }));
            }
        };

        rebuild();

        return window;
    }
}