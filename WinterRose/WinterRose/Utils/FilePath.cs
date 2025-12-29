using System;
using System.IO;

namespace WinterRose;

/// <summary>
/// A class representing a file or directory path
/// </summary>
public class FilePath
{
    string path = "";

    /// <summary>
    /// Creates a new FilePath object with the given path
    /// </summary>
    /// <param name="path"></param>
    public FilePath(string path)
    {
        path = path.Replace('/', Path.DirectorySeparatorChar)
                   .Replace('\\', Path.DirectorySeparatorChar);

        this.path = path;
    }

    /// <summary>
    /// Creates a new empty FilePath object
    /// </summary>
    public FilePath() { }

    public static implicit operator string(FilePath path)
    {
        return path.path;
    }

    public static implicit operator FilePath(string path)
    {
        return new FilePath(path);
    }

    /// <summary>
    /// Appends <paramref name="subPath"/> to <paramref name="path"/> no need for a seperator it will be added automatically. if it does exist already it will be removed.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="subPath"></param>
    /// <returns></returns>
    public static FilePath operator /(FilePath path, string subPath)
    {
        string basePath = path.path.TrimEnd(Path.DirectorySeparatorChar);
        subPath = subPath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        return new FilePath(basePath + Path.DirectorySeparatorChar + subPath);
    }


    /// <summary>
    /// Checks if the path points to a file
    /// </summary>
    /// <returns>True when the path points to a file, false if not</returns>
    public bool IsFile() => System.IO.File.Exists(path);
    /// <summary>
    /// Whether the file has the given <paramref name="extension"/>
    /// </summary>
    /// <param name="extension"></param>
    /// <returns>True if the file's extension matches <paramref name="extension"/>, otherwise false</returns>
    public bool HasExtension(string extension) => extension.StartsWith('.') ? path.EndsWith(extension) : path.EndsWith("." + extension);

    public string GetExtension() => System.IO.Path.GetExtension(path);

    /// <summary>
    /// Checks if the path points to a directory
    /// </summary>
    /// <returns>True when the path points to a directory, false if not</returns>
    public bool IsDirectory() => System.IO.Directory.Exists(path);

    /// <summary>
    /// Gets the Directory at the path
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public DirectoryInfo GetDirectory()
    {
        if (!IsDirectory())
            throw new Exception("Path does not lead to a directory");
        return new DirectoryInfo(path);
    }

    /// <summary>
    /// Gets the file at the path
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public FileInfo GetFile()
    {
        if (!IsFile())
            throw new Exception("Path does not lead to a file");
        return new FileInfo(path);
    }

    /// <summary>
    /// Gets the path
    /// </summary>
    /// <returns></returns>
    public override string ToString() => path;
    /// <summary>
    /// Checks if the path exists
    /// </summary>
    /// <returns></returns>
    public bool Exists() => IsFile() || IsDirectory();
    /// <summary>
    /// Opens the file at the path, if it is a directory or the file does not exist, returns null
    /// </summary>
    /// <param name="openMode"></param>
    /// <returns></returns>
    public FileStream? Open(FileMode openMode = FileMode.OpenOrCreate) => IsFile() ? new FileStream(path, openMode) : null;
    public override bool Equals(object obj) => obj is FilePath path && this.path == path.path;
    public override int GetHashCode() => HashCode.Combine(path);
}
