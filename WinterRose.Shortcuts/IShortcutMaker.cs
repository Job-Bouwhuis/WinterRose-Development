using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.FileManagement.Shortcuts;

/// <summary>
/// Represents a generic shortcut or launcher creator that can create files 
/// which launch executables or open files. 
/// Implementations may vary by platform:
/// - Windows: .lnk shortcut files
/// - Linux: .desktop files
/// - macOS: application launcher files
/// </summary>
public interface IShortcutMaker
{
    /// <summary>
    /// Creates a shortcut or launcher file at the specified <paramref name="shortcutPath"/> 
    /// pointing to the <paramref name="targetPath"/> file or executable. 
    /// This can be used to easily start an application or open a file, optionally providing
    /// arguments, a working directory, and an icon for the shortcut.
    /// </summary>
    /// <param name="shortcutPath">
    /// The path where the shortcut file will be created. <br></br>
    /// Must he a file path (not need to exist yet). you should omit  the file extension. since each OS may have different required extensions. it will be autoselected based on the OS.
    /// </param>
    /// <param name="targetPath">
    /// The path to the file or executable that the shortcut will launch or open. 
    /// </param>
    /// <param name="arguments">
    /// Optional command-line arguments to pass to the target when launched. 
    /// Defaults to <c>null</c> if no arguments are needed.
    /// </param>
    /// <param name="workingDirectory">
    /// Optional working directory for the shortcut. This sets the directory context 
    /// when the target is executed. Defaults to <c>null</c> to use the target's default directory.
    /// </param>
    /// <param name="iconPath">
    /// Optional path to an icon file to use for the shortcut. Defaults to <c>null</c>, 
    /// which uses the default icon of the target.
    /// </param>
    void CreateShortcut(
        string shortcutPath,
        string targetPath,
        string? arguments = null,
        string? workingDirectory = null,
        string? iconPath = null
    );
}

