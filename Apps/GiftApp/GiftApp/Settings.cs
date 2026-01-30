using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using WinterRose;
using WinterRose.EventBusses;
using WinterRose.FileManagement.Shortcuts;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.Recordium;
using WinterRose.WinterForgeSerializing;

namespace GiftApp;

public static class Settings
{
    private static Log log = new Log("Settings");

    public static TimeSpan HeartDisplayTime = TimeSpan.FromSeconds(3);
    public static TimeSpan FlowerDisplayTime = TimeSpan.FromSeconds(10);
    public static TimeSpan FlowerInterval = TimeSpan.FromMinutes(30);

    private static Windows.SystemTrayIcon trayIcon;
    private static readonly string APP_NAME = "GiftApp";

    public static void Init()
    {
        if (File.Exists("settings.wf"))
        {
            // winterforge has the ability to deserialize into static classes directly if serialized that way
            WinterForge.DeserializeFromFile("settings.wf");
        }
        else
        {
            Save();
        }

        try
        {
            trayIcon = new(ForgeWardenEngine.Current.Window.Handle, 0, "Gift App", "heart.ico");
            trayIcon.ShowInTray();
            trayIcon.Click.Subscribe(() =>
            {
                Program.Current.GlobalThreadLoom.InvokeOn("Main", () =>
                {
                    CreateWindow((Program)Program.Current);
                });
            });
        }
        catch (Exception ex)
        {
            log.Error("Failed to create system tray icon: " + ex.Message);
        }
    }

    public static void Shutdown()
    {
        Save();
        try
        {
            trayIcon.DeleteIcon();
        }
        catch (Exception ex)
        {
            log.Error("Failed to delete system tray icon: " + ex.Message);
        }
    }

    internal static void CreateWindow(Program p)
    {
        UIWindow window = new UIWindow("Settings", 765, 855);

        UIColumns cols = new();

        UIButton exitApp = new UIButton("Exit Gift App", (owner, btn) =>
        {
            owner.Close();
            IEnumerator waitForClose()
            {
                if(Program.Current != null)
                {
                    while(!((UIWindow)owner.Owner).IsFullyClosed)
                        yield return null;
                    Program.Current.Close();
                }
            }
            Program.Current.GlobalThreadLoom.InvokeOn("Main", waitForClose());
        });
        cols.AddToColumn(0, exitApp);

        
        UICheckBox autoStartup = new("Auto start with windows",
            Invocation.Create((IUIContainer c, UICheckBox b, bool newState) =>
            {
                b.IndicateBusy = true;
                newState = !IsStartupEnabled();
                SetStartup(newState).ContinueWith(t =>
                {
                    b.ForceSetChecked(IsStartupEnabled(), true);
                    if (b.Checked != newState)
                        Toasts.Error("Failed marking app to start with the OS");
                    else if (b.Checked)
                        Toasts.Success("The app will now start when your OS starts!");
                    else
                        Toasts.Success("The app will no longer start when your OS starts!");

                    b.IndicateBusy = false;
                });
               }), IsStartupEnabled());

        cols.AddToColumn(1, autoStartup);
        window.AddContent(cols);

        window.AddText("Heart Display Time (seconds):");
        UIValueSlider<double> heartDisplayTimeSlider = new UIValueSlider<double>(3, 30, HeartDisplayTime.TotalSeconds);
        heartDisplayTimeSlider.Step = 1;
        heartDisplayTimeSlider.HoldShiftToDisableSnap = false;
        heartDisplayTimeSlider.ValueChanged.Subscribe((newValue) =>
        {
            HeartDisplayTime = TimeSpan.FromSeconds(newValue);
        });
        window.AddContent(heartDisplayTimeSlider);

        window.AddText("Flower Display Time (seconds):");
        UIValueSlider<double> flowerDisplayTimeSlider = new UIValueSlider<double>(3, 30, FlowerDisplayTime.TotalSeconds);
        flowerDisplayTimeSlider.Step = 1;
        flowerDisplayTimeSlider.HoldShiftToDisableSnap = false;
        flowerDisplayTimeSlider.ValueChanged.Subscribe((newValue) =>
        {
            FlowerDisplayTime = TimeSpan.FromSeconds(newValue);
        });
        window.AddContent(flowerDisplayTimeSlider);

        window.AddText("Flower Interval (minutes):");
        UIValueSlider<double> flowerIntervalSlider = new UIValueSlider<double>(5, 120, FlowerInterval.TotalMinutes);
        flowerIntervalSlider.Step = 5;
        flowerIntervalSlider.HoldShiftToDisableSnap = false;
        flowerIntervalSlider.ValueChanged.Subscribe((newValue) =>
        {
            FlowerInterval = TimeSpan.FromMinutes(newValue);
        });
        window.AddContent(flowerIntervalSlider);

        UICircleProgress currentTimerProgress = new UICircleProgress(0, (self, current) =>
        {
            double total = p.GetCurrentTimerSeconds();
            float newProgress = (float)(p.time / total);

            TimeSpan remaining = TimeSpan.FromSeconds(total - p.time);
            if (remaining.Hours > 0)
                self.Text = $"{remaining:hh\\:mm\\:ss\\.f}";
            else
                self.Text = $"{remaining:mm\\:ss\\.f}";
            return newProgress;
        });
        currentTimerProgress.AlwaysShowText = true;
        currentTimerProgress.DontShowProgressPercent = true;

        window.AddText("Current Timer:");
        window.AddContent(currentTimerProgress);

        window.AddContent(new UIButton("Advance timer", (owner, btn) =>
        {
            p.time = (float)p.GetCurrentTimerSeconds();
        }));

        window.Show();
    }

    public static void Save()
    {
        WinterForge.SerializeStaticToFile(typeof(Settings), "settings.wf", TargetFormat.FormattedHumanReadable);
    }

    public static Task SetStartup(bool enable)
    {
        return ForgeWardenEngine.Current.GlobalThreadLoom.InvokeOn(ForgeWardenEngine.ENGINE_POOL_NAME, Task.Run(() =>
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SetStartupWindows(enable);
            }
            else
            {
                Toasts.Error("Startup management is not supported on this operating system.");
            }
        }));
    }

    static void SetStartupWindows(bool enable)
    {
        string shortcutPath = Path.Combine(ShortcutMaker.AutoStartupPath, $"{APP_NAME}");
        if (enable)
        {
            ShortcutMaker.CreateShortcut(
                shortcutPath,
                Environment.ProcessPath!,
                workingDirectory: Path.GetDirectoryName(Environment.ProcessPath)
            );
        }
        else
        {
            if (File.Exists(shortcutPath + ".lnk"))
                File.Delete(shortcutPath + ".lnk");
        }
    }

    public static bool IsStartupEnabled()
    {
        string shortcutPath = Path.Combine(ShortcutMaker.AutoStartupPath, $"{APP_NAME}.lnk");
        return File.Exists(shortcutPath);
    }
}
