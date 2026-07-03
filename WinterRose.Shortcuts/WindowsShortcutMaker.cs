using System.Diagnostics;

namespace WinterRose.Shortcuts;

internal sealed class WindowsShortcutMaker : IShortcutMaker
{
    public void CreateShortcut(
        string shortcutPath,
        string targetPath,
        string? arguments = null,
        string? workingDirectory = null,
        string? iconPath = null
    )
    {
        // Build the PowerShell command
        string psCommand = @"""
                            $WshShell = New-Object -ComObject WScript.Shell
                            $Shortcut = $WshShell.CreateShortcut('{0}')
                            $Shortcut.TargetPath = '{1}'
                            {2}
                            {3}
                            {4}
                            $Shortcut.Save()
                            Write-Output 'Shortcut created successfully'
                            """;

        string argumentsLine = string.IsNullOrEmpty(arguments) ? "" : $"$Shortcut.Arguments = '{arguments}'";
        string workingDirLine = string.IsNullOrEmpty(workingDirectory) ? "" : $"$Shortcut.WorkingDirectory = '{workingDirectory}'";
        string iconLine = string.IsNullOrEmpty(iconPath) ? "" : $"$Shortcut.IconLocation = '{iconPath}'";

        string finalCommand = string.Format(
            psCommand,
            shortcutPath.Replace("'", "''"),
            targetPath.Replace("'", "''"),
            argumentsLine,
            workingDirLine,
            iconLine
        );

        // Execute PowerShell
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -Command \"{finalCommand}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();

        string errors = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (!string.IsNullOrEmpty(errors))
            throw new Exception($"Error creating shortcut: {errors}");
    }
    
    public void CreateUriShortcut(
        string shortcutPath,
        string uri,
        string? iconPath = null
    )
    {
        string psCommand = @"""
                        $WshShell = New-Object -ComObject WScript.Shell
                        $Shortcut = $WshShell.CreateShortcut('{0}')
                        $Shortcut.TargetPath = '{1}'
                        {2}
                        $Shortcut.Save()
                        Write-Output 'URI shortcut created successfully'
                        """;

        string iconLine = string.IsNullOrEmpty(iconPath)
            ? ""
            : $"$Shortcut.IconLocation = '{iconPath}'";

        string finalCommand = string.Format(
            psCommand,
            shortcutPath.Replace("'", "''"),
            uri.Replace("'", "''"),
            iconLine
        );

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -Command \"{finalCommand}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();

        string errors = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (!string.IsNullOrEmpty(errors))
            throw new Exception($"Error creating URI shortcut: {errors}");
    }
}
