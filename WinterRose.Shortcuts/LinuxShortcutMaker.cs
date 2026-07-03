using System.Diagnostics;

namespace WinterRose.Shortcuts;

internal sealed class LinuxShortcutMaker : IShortcutMaker
{
    public void CreateShortcut(
        string shortcutPath,
        string targetPath,
        string? arguments = null,
        string? workingDirectory = null,
        string? iconPath = null
    )
    {
        // Ensure the shortcut file ends with .desktop
        if (!shortcutPath.EndsWith(".desktop"))
            shortcutPath += ".desktop";

        // Build the .desktop file content
        string desktopFileContent = 
            $@"[Desktop Entry]
Type=Application
Name={System.IO.Path.GetFileNameWithoutExtension(shortcutPath)}
Exec={targetPath}{(string.IsNullOrEmpty(arguments) ? "" : $" {arguments}")}
{(string.IsNullOrEmpty(workingDirectory) ? "" : $"Path={workingDirectory}")}
{(string.IsNullOrEmpty(iconPath) ? "" : $"Icon={iconPath}")}
Terminal=false
";

        // Write to file
        System.IO.File.WriteAllText(shortcutPath, desktopFileContent);

        // Make it executable
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/chmod",
                Arguments = $"+x \"{shortcutPath}\"",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string errors = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (!string.IsNullOrEmpty(errors))
            throw new Exception($"Error setting shortcut executable: {errors}");
    }
    
    public void CreateUriShortcut(
        string shortcutPath,
        string uri,
        string? iconPath = null
    )
    {
        if (!shortcutPath.EndsWith(".desktop"))
            shortcutPath += ".desktop";

        string desktopFileContent =
            $@"[Desktop Entry]
Type=Link
Name={System.IO.Path.GetFileNameWithoutExtension(shortcutPath)}
URL={uri}
{(string.IsNullOrEmpty(iconPath) ? "" : $"Icon={iconPath}")}
";

        System.IO.File.WriteAllText(shortcutPath, desktopFileContent);
    }
}
