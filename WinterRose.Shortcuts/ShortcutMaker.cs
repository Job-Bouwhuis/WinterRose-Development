using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.FileManagement.Shortcuts;

/// <summary>
/// A static factory class for creating shortcuts. it automatically selects the appropriate implementation based on the operating system.
/// </summary>
public static class ShortcutMaker
{
    public static readonly string AutoStartupPath = OperatingSystem.IsWindows()
        ? Environment.GetFolderPath(Environment.SpecialFolder.Startup)
        : throw new NotSupportedException("This operating system is not supported.");

    /// <inheritdoc cref="IShortcutMaker.CreateShortcut(string, string, string?, string?, string?)"/>
    public static void CreateShortcut(string shortcutPath, string targetPath, string? arguments = null, string? workingDirectory = null, string? iconPath = null)
    {
        IShortcutMaker shortcutMaker;

        if(Path.GetExtension(shortcutPath) != string.Empty)
            shortcutPath = Path.ChangeExtension(shortcutPath, null);

        if (OperatingSystem.IsWindows())
        {
            shortcutPath = Path.ChangeExtension(shortcutPath, ".lnk");
            shortcutMaker = new WindowsShortcutMaker();
        }
        else if (OperatingSystem.IsLinux())
        {
            shortcutPath = Path.ChangeExtension(shortcutPath, ".desktop");
            shortcutMaker = new LinuxShortcutMaker();
        }
        else
            throw new NotSupportedException("This operating system is not supported.");

        shortcutMaker.CreateShortcut(shortcutPath, targetPath, arguments, workingDirectory, iconPath);
    }
}
