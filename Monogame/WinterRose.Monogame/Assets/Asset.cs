using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.Monogame;

/// <summary>
/// A base class for all assets
/// </summary>
public abstract class Asset
{
    /// <summary>
    /// The name of the asset.
    /// </summary>
    [IncludeWithSerialization]
    public string Name { get; internal set; }
    /// <summary>
    /// Creates a new <see cref="Asset"/> with the specified name.
    /// <br></br>Throws an <see cref="Exception"/> if an asset with the same name already exists.
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="Exception"></exception>
    public Asset(string name)
    {
        Name = name;

        if (string.IsNullOrWhiteSpace(name))
            return;
    }

    /// <summary>
    /// The file that is associated with this asset.
    /// </summary>
    [Hide]
    public AssetDatabaseFile File
    {
        get
        {
            if (file != null)
                return file;

             return file = AssetDatabase.ContentFolder.GetFile(Name, GetType());
        }
    }
    private AssetDatabaseFile file;

    /// <summary>
    /// When overriden, loads the asset from the file.
    /// </summary>
    public abstract void Load();
    /// <summary>
    /// When overriden, unloads the asset from memory.
    /// </summary>
    public abstract void Unload();
    /// <summary>
    /// When overriden, saves the asset to the file.
    /// </summary>
    public abstract void Save();
}
