using System.Runtime.CompilerServices;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.Recordium;
using WinterRose.WinterForgeSerializing;
using WinterRoseUtilityApp.MailReader.AuthHandlers;
using WinterRoseUtilityApp.MailReader.Models;

namespace WinterRoseUtilityApp.MailReader;

public class EmailAuthManager : IAssetHandler<List<EmailAccount>>
{
    private static readonly Log log = new Log("MailWatcher");

    private const string ASSET_NAME = "EmailAccounts";
    private static readonly object SYNC_OBJECT = new();

    private static List<EmailAccount> accounts = new();

    public static string[] InterestedInExtensions => [".emails"];

    public EmailAuthManager()
    {
        LoadAccounts();
    }

    private static void LoadAccounts()
    {
        lock (SYNC_OBJECT)
        {
            if (accounts.Count > 0)
                return;

            if (Assets.Exists(ASSET_NAME))
            {
                accounts = Assets.Load<List<EmailAccount>>(ASSET_NAME);
            }
            else
            {
                accounts = new List<EmailAccount>();
                Assets.CreateAsset(accounts, ASSET_NAME);
            }
        }
    }

    private static void SaveAccounts()
    {
        lock (SYNC_OBJECT)
        {
            Assets.Save(ASSET_NAME, accounts);
        }
    }

    public static IReadOnlyList<EmailAccount> GetSavedAccounts()
    {
        lock (SYNC_OBJECT)
        {
            if (accounts.Count == 0)
                LoadAccounts();
            return accounts.AsReadOnly();
        }
    }

    public static void SaveAccount(EmailAccount account)
    {
        lock (SYNC_OBJECT)
        {
            var existing = accounts.FirstOrDefault(a => a.Address == account.Address && a.Provider == account.Provider);
            if (existing != null)
                accounts.Remove(existing);

            accounts.Add(account);
            SaveAccounts();
        }
    }

    public static async Task<bool> TryLoginAsync(string provider, string email = "")
    {
        try
        {
            switch (provider)
            {
                case "Outlook":
                    {
                        // See if we already have a saved account for this email
                        var savedAccount = GetSavedAccounts()
                            .FirstOrDefault(a => a.Provider == "Outlook" && a.Address == email);

                        if (savedAccount != null)
                        {
                            // Try silent login first
                            bool silentSuccess = await OutlookAuthHandler.LoginAsync(savedAccount);
                            if (silentSuccess)
                                return true;

                            return false;
                        }

                        // Either no saved account, or silent login failed -> fallback to interactive login
                        return await OutlookAuthHandler.LoginAsync(email);
                    }
                case "Gmail":
                    throw new NotImplementedException();
                case "ProtonMail":
                    throw new NotImplementedException();
                default:
                    throw new InvalidOperationException("Unsupported mail provider: " + provider);
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, $"Login attempt for {email} failed");
            return false;
        }
    }

    public static void PromptLoginIfNeeded()
    {
        var savedAccounts = Assets.Load<List<EmailAccount>>("EmailAccounts");
        if (savedAccounts == null || savedAccounts.Count == 0)
        {
            ContainerCreators.AddEmailDialog("No email accounts configured. Add one to receive notifications.");
            return;
        }

        foreach (var acc in savedAccounts)
        {
            if (!TryAutoAuthenticate(acc))
            {
                ContainerCreators.AddEmailDialog($"Session expired for {acc.Address}. Please log in again.");
            }
        }
    }

    private static bool TryAutoAuthenticate(EmailAccount account)
    {
        try
        {
            // Use provider-specific refresh token logic here later
            return account.LastAuthTime.AddDays(7) > DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            log.Warning(ex, $"Auto-auth failed for {account.Address}");
            return false;
        }
    }

    public static bool InitializeNewAsset(AssetHeader header) => throw new NotImplementedException();
    public static List<EmailAccount> LoadAsset(AssetHeader header)
    {
        lock (SYNC_OBJECT)
        {
            var result = WinterForge.DeserializeFromFile(header.Path);
            if (result is not List<EmailAccount>)
            {
                log.Info("No accounts found!");
                accounts = [];
                return accounts;
            }

            accounts = Unsafe.As<List<EmailAccount>>(result)!;
            return accounts;
        }
    }
    public static bool SaveAsset(AssetHeader header, List<EmailAccount> asset)
    {
        lock (SYNC_OBJECT)
        {
            try
            {
                WinterForge.SerializeToFile(asset, header.Path);
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to save email accounts (passwords arent included!!!)");
                return false;
            }
        }
    }
    public static bool SaveAsset(string assetName, List<EmailAccount> asset)
    {
        if (!Assets.Exists(assetName))
            Assets.CreateAsset<List<EmailAccount>>([], assetName);
        return SaveAsset(Assets.GetHeader(assetName), asset);
    }
}
