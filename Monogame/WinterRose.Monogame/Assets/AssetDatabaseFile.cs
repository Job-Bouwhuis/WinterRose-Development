using System.Collections.Generic;
using System.IO;
using WinterRose.FileManagement;

namespace WinterRose.Monogame;

/// <summary>
/// A file in the AssetDatabase
/// </summary>
public sealed class AssetDatabaseFile
{
    public FileInfo File { get; init; }
    public AssetDatabaseFolder Folder { get; init; }
    public AssetDatabaseFile(AssetDatabaseFolder folder, FileInfo file)
    {
        File = file;
        Folder = folder;
    }

    /// <summary>
    /// Reads the content of the file.
    /// </summary>
    /// <returns></returns>
    public string ReadContent()
    {
        return FileManager.Read(File.FullName.Replace("\n", "").Replace("\r", ""));
    }

    /// <summary>
    /// Writes the content to the file.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="override"></param>
    public void WriteContent(string content, bool @override = false)
    {
        FileManager.WriteLine(File.FullName.Replace("\n", "").Replace("\r", ""), content, @override);
    }

    /// <summary>
    /// Enumerates over the lines of the files and returns them as a tuple of the line and the line number.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<(string, int)> GetNumberedEnumerator()
    {
        return FileManager.EnumerateNumberedLines(File.FullName.Replace("\n", "").Replace("\r", ""));
    }

    /// <summary>
    /// Enumerates over the lines of the file.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<string> GetEnumerator()
    {
        return FileManager.EnumerateLines(File.FullName.Replace("\n", "").Replace("\r", ""));
    }
}