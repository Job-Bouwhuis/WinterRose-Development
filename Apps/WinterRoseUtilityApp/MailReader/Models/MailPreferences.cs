using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.WinterForgeSerializing;

namespace WinterRoseUtilityApp.MailReader.Models;

public class MailPreferences : IAssetHandler<MailPreferences>
{
    public static string[] InterestedInExtensions => [];

    public int CheckIntervalMinutes { get; set; } = 30;

    public HashSet<string> ImportantSenders { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> IgnoredSenders { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public static bool InitializeNewAsset(AssetHeader header)
    {
        MailPreferences defaultPrefs = new();
        defaultPrefs.ImportantSenders.Add("karinbouwhuis@hotmail.com");
        defaultPrefs.ImportantSenders.Add("info@syntaxis.nl");
            
        defaultPrefs.IgnoredSenders.Add("close_friend_updates@facebookmail.com");
        defaultPrefs.IgnoredSenders.Add("messages-noreply@linkedin.com");
        defaultPrefs.IgnoredSenders.Add("notifications-noreply@linkedin.com");
        defaultPrefs.IgnoredSenders.Add("githubeducation@github.com");
        defaultPrefs.IgnoredSenders.Add("notifications@github.com");
        defaultPrefs.IgnoredSenders.Add("notification@codacy.com");
        defaultPrefs.IgnoredSenders.Add("noreply@mijnkombijdepolitie.nl");
        defaultPrefs.IgnoredSenders.Add("jbl@em.jbl.com");
        defaultPrefs.IgnoredSenders.Add("support@nuget.org");
        defaultPrefs.IgnoredSenders.Add("updates-noreply@linkedin.com");
        defaultPrefs.IgnoredSenders.Add("pageupdates@facebookmail.com");
        defaultPrefs.IgnoredSenders.Add("friendupdates@facebookmail.com");
        defaultPrefs.IgnoredSenders.Add("info@computermantwente.nl");
        defaultPrefs.IgnoredSenders.Add("newsletter@coderabbit.ai");
        defaultPrefs.IgnoredSenders.Add("penningmeester@syntaxis.nl");
        defaultPrefs.IgnoredSenders.Add("cmd-r@wooting.io");
        defaultPrefs.IgnoredSenders.Add("_noreply@odido.nl");
        defaultPrefs.IgnoredSenders.Add("updates-noreply@linkedin.com");
        defaultPrefs.IgnoredSenders.Add("editors-noreply@linkedin.com");

        SaveAsset(header, defaultPrefs);

        return true;
    }

    public static MailPreferences LoadAsset(AssetHeader header)
    {
        object obj = WinterForge.DeserializeFromFile(header.Path);
        if (obj is Nothing)
            InitializeNewAsset(header);

        obj = WinterForge.DeserializeFromFile(header.Path);
        if (obj is Nothing)
            throw new InvalidOperationException("Failed to load mail preferences");

        return (MailPreferences)obj;
    }

    public static bool SaveAsset(AssetHeader header, MailPreferences asset)
    {
        WinterForge.SerializeToFile(asset, header.Path);
        return true;
    }
    public static bool SaveAsset(string assetName, MailPreferences asset)
    {
        AssetHeader header = Assets.GetHeader(assetName);
        return SaveAsset(header, asset);
    }
}
