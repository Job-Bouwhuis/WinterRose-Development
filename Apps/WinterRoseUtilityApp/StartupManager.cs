using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FileManagement.Shortcuts;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;

namespace WinterRoseUtilityApp;

public static class StartupManager
{
    static readonly string APP_NAME = "WinterRoseUtilApp";
    static readonly string EXEC_PATH = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

    public static Task SetStartup(bool enable)
    {
        return ForgeWardenEngine.Current.GlobalThreadLoom.InvokeOn(ForgeWardenEngine.ENGINE_POOL_NAME, Task.Run(() =>
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SetStartupWindows(enable);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                SetStartupLinux(enable);
            }
            else
            {
                Toasts.Error("Startup management is not supported on this operating system.");
            }
        }));
    }

    public static bool IsStartupEnabled()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return IsStartupEnabledWindows();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return IsStartupEnabledLinux();
        return false;
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
            if (File.Exists(shortcutPath))
                File.Delete(shortcutPath);
        }
    }

    static bool IsStartupEnabledWindows()
    {
        string shortcutPath = Path.Combine(ShortcutMaker.AutoStartupPath, $"{APP_NAME}.lnk");
        return File.Exists(shortcutPath);
    }

    static void SetStartupLinux(bool enable)
    {
        string autostartDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".config", "autostart");
        Directory.CreateDirectory(autostartDir);

        string desktopFile = Path.Combine(autostartDir, $"{APP_NAME}.desktop");

        if (enable)
        {
            string desktopEntry = $@"
                [Desktop Entry]
                Type=Application
                Exec={EXEC_PATH}
                Hidden=false
                NoDisplay=false
                X-GNOME-Autostart-enabled=true
                Name={APP_NAME}
                Comment=Auto-start {APP_NAME} at login
                ";
            File.WriteAllText(desktopFile, desktopEntry.Trim());
        }
        else
        {
            if (File.Exists(desktopFile))
                File.Delete(desktopFile);
        }
    }

    static bool IsStartupEnabledLinux()
    {
        string desktopFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            ".config", "autostart", $"{APP_NAME}.desktop");
        return File.Exists(desktopFile);
    }
}
