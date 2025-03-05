using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame;

/// <summary>
/// A folder in the AssetDatabase
/// </summary>
public sealed class AssetDatabaseFolder
{
    /// <summary>
    /// Whether or not to create the item if it doesn't exist or throw an exception
    /// </summary>
    public static bool CreateItemIfMissing { get; set; } = true;

    /// <summary>
    /// The directory this folder represents
    /// </summary>
    public DirectoryInfo Directory { get; init; }
    /// <summary>
    /// Creates a new <see cref="AssetDatabaseFolder"/>
    /// </summary>
    /// <param name="directory"></param>
    public AssetDatabaseFolder(DirectoryInfo directory)
    {
        Directory = directory;
        if (!Directory.Exists)
            Directory.Create();
    }

    /// <summary>
    /// Gets a subfolder in this folder
    /// </summary>
    /// <param name="name">the name of the folder to search for</param>
    /// <returns></returns>
    public AssetDatabaseFolder GetFolder(string name)
    {
        DirectoryInfo info = Directory.GetDirectories().FirstOrDefault(x => x.Name == name);
        if (info is null)
            if (CreateItemIfMissing)
                info = Directory.CreateSubdirectory(name);
            else
                throw new FileNotFoundException($"Folder {name} not found");

        return new AssetDatabaseFolder(info);
    }

    /// <summary>
    /// Gets a file in this folder
    /// </summary>
    /// <param name="name">The name of the file to search for</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public AssetDatabaseFile GetFile<T>(string name) where T : Asset
    {
        string fileName = name + '.' + typeof(T).Name;
        FileInfo info = Directory.GetFiles().FirstOrDefault(x => x.Name == fileName);
        if (info is null)
            if (CreateItemIfMissing)
                info = new FileInfo(Path.Combine(Directory.FullName, fileName));
            else
                throw new FileNotFoundException($"File {fileName} not found");

        return new AssetDatabaseFile(this, info);
    }

    /// <summary>
    /// Gets a file in this folder
    /// </summary>
    /// <param name="name">The name of the file to search for</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public AssetDatabaseFile GetFile(string name, Type t)
    {
        string fileName = name + '.' + t.Name;
        FileInfo info = Directory.GetFiles().FirstOrDefault(x => x.Name == fileName);
        if (info is null)
            if (CreateItemIfMissing)
                info = new FileInfo(Path.Combine(Directory.FullName, fileName));
            else
                throw new FileNotFoundException($"File {fileName} not found");

        return new AssetDatabaseFile(this, info);
    }
}
