using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WinterRose.Utils;

public static partial class OS
{
    public static bool OpenFile(out string file, string title = "Open File",
        string filter = "All Files (*.*)\0*.*\0", string initialDirectory = null, bool showHidden = false)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Windows.OpenFile(out file, title, filter, initialDirectory ?? "C:\\", showHidden);
        else
            return LinuxFileDialog.OpenFile(out file, title, filter, initialDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), showHidden);
    }

    public static bool SaveFile(out string file, string title = "Save File",
        string filter = "All Files (*.*)\0*.*\0", string initialDirectory = null, string defaultExtension = null, bool showHidden = false)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Windows.SaveFile(out file, title, filter, initialDirectory ?? "C:\\", defaultExtension, showHidden);
        else
            return LinuxFileDialog.SaveFile(out file, title, filter, initialDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), defaultExtension, showHidden);
    }

    public static bool OpenFolder(out string folder, string title = "Open Folder")
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Windows.OpenFolder(out folder, title);
        else
            return LinuxFileDialog.OpenFolder(out folder, title);
    }

    public static void PCShutdown()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Windows.PCShutdown();
        else
            Process.Start("shutdown", "-h now");
    }

    public static void PCRestart()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Windows.PCRestart();
        else
            Process.Start("shutdown", "-r now");
    }

    public static void PCLock()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Windows.PCLock();
        else
            Process.Start("loginctl", "lock-session");
    }

}

public static class LinuxFileDialog
{
    public static bool OpenFile(out string file, string title, string filter, string initialDirectory, bool showHidden)
    {
        string cmd = $"kdialog --getopenfilename \"{initialDirectory}/\" \"{filter}\" --title \"{title}\"";
        if (showHidden) cmd += " --hidden";
        var result = ShellCommand.Run(cmd);
        file = result.ExitCode == 0 ? result.Output.Trim() : null;
        return result.ExitCode == 0;
    }

    public static bool SaveFile(out string file, string title, string filter, string initialDirectory, string defaultExtension, bool showHidden)
    {
        string cmd = $"kdialog --getsavefilename \"{initialDirectory}/\" \"{filter}\" --title \"{title}\"";
        var result = ShellCommand.Run(cmd);
        file = result.ExitCode == 0 ? result.Output.Trim() : null;
        return result.ExitCode == 0;
    }

    public static bool OpenFolder(out string folder, string title)
    {
        string cmd = $"kdialog --getexistingdirectory \"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\" --title \"{title}\"";
        var result = ShellCommand.Run(cmd);
        folder = result.ExitCode == 0 ? result.Output.Trim() : null;
        return result.ExitCode == 0;
    }
}

