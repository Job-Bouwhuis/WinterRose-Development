using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;

namespace WinterRose.Monogame;

/// <summary>
/// A database of assets that can be loaded and unloaded at will.
/// </summary>
public static class AssetDatabase
{
    /// <summary>
    /// A dictionary of all loaded assets.
    /// </summary>
    public static Dictionary<string, Asset> Assets { get; } = new();
    /// <summary>
    /// The root folder of the asset database.
    /// </summary>
    public static AssetDatabaseFolder ContentFolder { get; internal set; }

    static AssetDatabase()
    {
        ContentFolder = new AssetDatabaseFolder(new DirectoryInfo("Content/AssetDatabase"));
    }

    /// <summary>
    /// Declare an asset to the database.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Asset DeclareAsset<T>(T data) where T : Asset
    {
        if (Assets.ContainsKey(data.Name))
            throw new Exception($"Asset {data.Name} already declared");

        data.Save();

        Assets.Add(data.Name, data);

        return data;
    }

    /// <summary>
    /// Load an asset from the database.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns>The loaded asset</returns>
    /// <exception cref="Exception"/>
    /// <exception cref="FileNotFoundException"/>
    public static T LoadAsset<T>(string name) where T : Asset
    {
        if (!Assets.ContainsKey(name))
        {
            AssetDatabaseFile file = ContentFolder.GetFile(name);

            T asset = ActivatorExtra.CreateInstance<T>(name);

            asset.Load();   

            Assets.Add(name, asset);
            return asset;
        }
        Assets.TryGetValue(name, out Asset value);
        if (value is T t)
            return t;

        throw new Exception($"Asset {name} is not of type {typeof(T).Name}, it is of type {value.GetType().Name}");
    }

    /// <summary>
    /// If the asset is loaded, return it, otherwise return null.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Asset? Asset(string name)
    {
        if (Assets.TryGetValue(name, out Asset value))
            return value;
        return null;
    }

    /// <summary>
    /// Unload an asset from the database.
    /// </summary>
    /// <param name="name"></param>
    public static void UnloadAsset(string name)
    {
        if (Assets.ContainsKey(name))
        {
            Assets[name].Unload();
            Assets.Remove(name);
        }
    }

    /// <summary>
    /// Unload all assets from the database.
    /// </summary>
    public static void UnloadAll()
    {
        foreach (var asset in Assets)
            asset.Value.Unload();
        Assets.Clear();
    }

    /// <summary>
    /// Save an asset to the database.
    /// </summary>
    /// <param name="name"></param>
    public static void SaveAsset(string name)
    {
        if (Assets.ContainsKey(name))
            Assets[name].Save();
    }

    /// <summary>
    /// Save all assets to the database.
    /// </summary>
    public static void SaveAll()
    {
        foreach (var asset in Assets)
            asset.Value.Save();
    }

    /// <summary>
    /// Reload an asset from the database.
    /// </summary>
    /// <param name="name"></param>
    public static void ReloadAsset(string name)
    {
        if (Assets.TryGetValue(name, out Asset? value))
        {
            value.Unload();
            value.Load();
        }
    }

    public static bool AssetExists(string assetPath)
    {
        assetPath = assetPath.Replace("\\", "/");
        string[] path = assetPath.Split('/');
        DirectoryInfo currentDir = ContentFolder.Directory;
        FileInfo[] files = currentDir.GetFiles(assetPath, SearchOption.AllDirectories);
        return files.Length > 0;
    }
}
