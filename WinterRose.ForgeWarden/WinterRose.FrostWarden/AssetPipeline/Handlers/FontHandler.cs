using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.ForgeWarden.AssetPipeline.Handlers;

internal class FontHandler : IAssetHandler<Font>
{
    public static string[] InterestedInExtensions => [".ttf"];

    public static bool InitializeNewAsset(AssetHeader header) => true;
    public static Font LoadAsset(AssetHeader header)
    {
        using var source = header.Source;
        return ray.LoadFont(source.Name);
    }
    public static bool SaveAsset(AssetHeader header, Font asset) => throw new InvalidOperationException("Fonts are readonly");
    public static bool SaveAsset(string assetName, Font asset) => throw new InvalidOperationException("Fonts are readonly");
}
