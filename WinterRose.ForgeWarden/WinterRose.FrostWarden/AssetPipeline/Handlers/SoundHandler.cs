using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.Recordium;

namespace WinterRose.ForgeWarden.AssetPipeline.Handlers;

internal class SoundHandler : IAssetHandler<Sound>
{
    public static string[] InterestedInExtensions => [".wav", ".mp3", ".ogg"];

    public static Sound LoadAsset(AssetHeader header)
    {
        using var s = header.Source;
        return Raylib.LoadSound(s.Name);
    }

    public static bool InitializeNewAsset(AssetHeader header) => true;
    public static bool SaveAsset(AssetHeader header, Sound asset)
    {
        new Log("Sound Asset Handler").Warning("Saving a \"Sound\" asset not supported.", null, 0);
        return true;
    }
    public static bool SaveAsset(string assetName, Sound asset)
    {
        new Log("Sound Asset Handler").Warning("Saving a \"Sound\" asset not supported.", null, 0);
        return true;
    }
}
