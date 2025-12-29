using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.WinterForgeSerializing;

namespace WinterRoseUtilityApp.DrinkReminder;

internal class DrinkAssetHandler : IAssetHandler<DrinkSettings>
{
    public static string[] InterestedInExtensions => [".drinksettings"];

    public static bool InitializeNewAsset(AssetHeader header)
    {
        return SaveAsset(header, DrinkSettings.Default);
    }
    public static DrinkSettings LoadAsset(AssetHeader header)
    {
        object obj = WinterForge.DeserializeFromFile(header.Path);
        if (obj is Nothing)
        {
            SaveAsset(header, DrinkSettings.Default);
            return DrinkSettings.Default;
        }
        return obj as DrinkSettings ?? DrinkSettings.Default;
    }
    public static bool SaveAsset(AssetHeader header, DrinkSettings asset)
    {
        WinterForge.SerializeToFile(asset, header.Path);
        return true;
    }

    public static bool SaveAsset(string assetName, DrinkSettings asset)
    {
        AssetHeader header = Assets.GetHeader(assetName);
        return SaveAsset(header, asset);
    }

}
