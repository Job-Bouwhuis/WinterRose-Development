using System.Globalization;
using System.Text;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.Recordium;

namespace GiftApp;

internal class WeightedMessageListHandler : IAssetHandler<Dictionary<string, double>>
{
    private static Log log = new Log("WeightedMessageListHandler");
    public static string[] InterestedInExtensions => [".txt"];

    public static bool InitializeNewAsset(AssetHeader header)
    {
        return true;
    }

    public static Dictionary<string, double> LoadAsset(AssetHeader header)
    {
        using StreamReader reader = new StreamReader(header.Source, Encoding.UTF8);

        Dictionary<string, double> messages = new Dictionary<string, double>();

        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            int splitIndex = line.LastIndexOf('@');
            if (splitIndex <= 0 || splitIndex == line.Length - 1)
                continue;

            string message = line[..splitIndex];
            string weightText = line[(splitIndex + 1)..];

            if (!double.TryParse(weightText, NumberStyles.Any, CultureInfo.InvariantCulture, out double weight))
            {
                weight = 1;
                log.Warning(weightText + " could not be parsed to a double");
            }
            messages[message] = weight;
        }

        return messages;
    }
    public static bool SaveAsset(AssetHeader header, Dictionary<string, double> asset)
    {
        if (header.IsReadOnly)
            return false;

        using StreamWriter writer = new StreamWriter(header.Path, false, Encoding.UTF8);
        foreach (KeyValuePair<string, double> pair in asset)
            writer.WriteLine($"{pair.Key}@{pair.Value.ToString(CultureInfo.InvariantCulture)}");

        writer.Flush();
        return true;
    }
    public static bool SaveAsset(string assetName, Dictionary<string, double> asset) => SaveAsset(Assets.GetHeader(assetName), asset);
}
