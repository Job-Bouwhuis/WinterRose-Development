namespace WinterRose.Shortcuts;

/// <summary>
/// Represents a generic shortcut or launcher creator that can create files 
/// which launch executables or open files. 
/// Implementations may vary by platform:
/// - Windows: .lnk shortcut files
/// - Linux: .desktop files
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

    /// <summary>
    /// Creates a shortcut or launcher file at the specified <paramref name="shortcutPath"/> 
    /// that points to a URI (such as a website, deep link, or custom protocol handler).
    /// </summary>
    /// <remarks>
    /// This method is intended for creating web or protocol-based shortcuts rather than
    /// file or executable launchers. The resulting shortcut will open the provided URI
    /// using the system's default handler (e.g., a browser for https links).
    /// <br/><br/>
    /// The actual shortcut format is platform-dependent:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// On Windows, a standard <c>.lnk</c> file is created with the target set to the URI.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// On Linux, a <c>.desktop</c> file with <c>Type=Link</c> is generated.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <param name="shortcutPath">
    /// The path where the shortcut file will be created. <br/>
    /// This should be a file path without an extension. The correct extension will be
    /// automatically applied based on the current operating system.
    /// </param>
    /// <param name="uri">
    /// The URI that the shortcut will open when executed. <br/>
    /// This can be a web URL (e.g. <c>https://example.com</c>), a deep link,
    /// or any protocol handled by the operating system.
    /// </param>
    /// <param name="iconPath">
    /// Optional path to an icon file used to visually represent the shortcut.
    /// If <c>null</c>, the system default icon for the URI type will be used.
    /// </param>
    /// <exception cref="System.Exception">
    /// Thrown if the underlying platform-specific shortcut creation process fails.
    /// </exception>
    void CreateUriShortcut(
        string shortcutPath,
        string uri,
        string? iconPath = null
    );
}

