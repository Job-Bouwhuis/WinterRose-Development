using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.AssetPipeline;

namespace GiftApp;

internal class MessageListHandler : IAssetHandler<List<string>>
{
    public static string[] InterestedInExtensions => [".txt"];

    public static bool InitializeNewAsset(AssetHeader header)
    {
        return true;
    }

    public static List<string> LoadAsset(AssetHeader header)
    {
        using StreamReader reader = new StreamReader(header.Source, Encoding.UTF8);

        List<string> messages = new List<string>();
        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();
            if (line is not null)
                messages.Add(line);
        }
        return messages;
    }
    public static bool SaveAsset(AssetHeader header, List<string> asset)
    {
        if (header.IsReadOnly)
            return false;

        using StreamWriter writer = new StreamWriter(header.Path, false, Encoding.UTF8);
        foreach (string message in asset)
            writer.WriteLine(message);
        writer.Flush();
        return true;
    }
    public static bool SaveAsset(string assetName, List<string> asset) => SaveAsset(Assets.GetHeader(assetName), asset);
}