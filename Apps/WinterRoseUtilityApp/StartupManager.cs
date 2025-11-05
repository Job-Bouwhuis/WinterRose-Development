using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinterRoseUtilityApp;
public static class StartupManager
{
    static readonly string APP_NAME = "WinterRoseUtilApp";
    static readonly string EXEC_PATH = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

    public static void SetStartup(bool enable)
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
            Console.WriteLine("Startup registration not supported on this OS.");
        }
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
        using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        if (enable)
        {
            key.SetValue(APP_NAME, $"\"{EXEC_PATH}\"");
        }
        else
        {
            key.DeleteValue(APP_NAME, false);
        }
    }

    static bool IsStartupEnabledWindows()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
        return key?.GetValue(APP_NAME) != null;
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
