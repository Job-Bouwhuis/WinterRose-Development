using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Workers;

namespace WinterRose.ForgeWarden.AssetPipeline;

public interface IAssetHandler<T>
{
    /// <summary>
    /// Allows the handler to specify which file extensions it handles. Used when creating new asset headers from arbitrary files.
    /// </summary>
    public static abstract string[] InterestedInExtensions { get; }

    /// <summary>
    /// Initializes a new asset. who's file extention was in the <see cref="InterestedInExtensions"/>.
    /// </summary>
    /// <param name="header"></param>
    /// <returns></returns>
    public static abstract bool InitializeNewAsset(AssetHeader header);

    /// <summary>
    /// Loads the asset from the specified header.
    /// </summary>
    /// <param name="header">The asset header to load the asset from</param>
    /// <returns>The loaded asset, or null when failed</returns>
    public static abstract T LoadAsset(AssetHeader header);

    /// <summary>
    /// Saves the asset to the specified header.
    /// </summary>
    /// <param name="header">The asset header to save the asset to.</param>
    /// <param name="asset">The asset to save.</param>
    /// <returns>True when successfully saved, false if not</returns>
    public static abstract bool SaveAsset(AssetHeader header, T asset);

    /// <summary>
    /// Saves the asset using the specified name.
    /// </summary>
    /// <param name="assetName">The name to use when saving the asset</param>
    /// <param name="asset">The asset to save.</param>
    /// <returns>True when successfully saved, false if not</returns>
    public static abstract bool SaveAsset(string assetName, T asset);
}
