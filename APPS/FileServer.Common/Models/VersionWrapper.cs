using WinterRose.Serialization;

namespace WinterRose.FileServer.Common.Models;

/// <summary>
/// A wrapper for the <see cref="System.Version"/> class. This is needed to serialize the <see cref="System.Version"/> class correctly.
/// </summary>
public class VersionWrapper
{
    /// <summary>
    /// the <see cref="Version"/> class.
    /// </summary>
    [IncludePrivateFieldsForField]
    public Version Version;

    /// <summary>
    /// Implicitly converts a <see cref="VersionWrapper"/> to a <see cref="System.Version"/>.
    /// </summary>
    /// <param name="wrapper"></param>
    public static implicit operator Version(VersionWrapper wrapper) => wrapper.Version;

    /// <summary>
    /// Implicitly converts a <see cref="System.Version"/> to a <see cref="VersionWrapper"/>.
    /// </summary>
    /// <param name="version"></param>
    public static implicit operator VersionWrapper(Version version) => new() { Version = version };
}