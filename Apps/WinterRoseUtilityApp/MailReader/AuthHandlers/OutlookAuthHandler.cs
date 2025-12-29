using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.Recordium;
using WinterRoseUtilityApp.MailReader.Models;

namespace WinterRoseUtilityApp.MailReader.AuthHandlers;
internal static class OutlookAuthHandler
{
    public const string CLIENT_ID = "7bbef0bd-5469-49d8-9909-f3ba5b7746bd"; // You'll register this app in Azure
    public static readonly string[] SCOPES =
    [
        "Mail.Read",
        "User.Read",
        "offline_access",
        "Mail.ReadWrite"
    ];

    private static readonly Log log = new Log("OutlookAuthHandler");

    public static async Task<bool> LoginAsync(string email)
    {
        try
        {
            var clientId = CLIENT_ID;

            // Build the MSAL public client application
            IPublicClientApplication app = BuildOutlookApp(clientId);

            // Acquire token interactively
            var result = await app.AcquireTokenInteractive(SCOPES)
                .WithLoginHint(email)
                .ExecuteAsync();

            if (result == null)
            {
                log.Error("Outlook Login result was null.");
                return false;
            }

            var account = new EmailAccount(
                provider: "Outlook",
                address: result.Account.Username ?? email,
                displayName: result.Account.Username ?? email, // fallback
                accessToken: result.AccessToken,
                lastAuthTime: DateTime.UtcNow,
                tokenExpiry: result.ExpiresOn.UtcDateTime,
                tenantId: result.TenantId,
                scopes: string.Join(",", result.Scopes),
                accountId: result.Account.HomeAccountId.Identifier
            );

            SaveAccount(account);
            log.Info($"Outlook account '{account.Address}' connected successfully.");
            return true;
        }
        catch (MsalException ex)
        {
            log.Error(ex, "MSAL authentication failed");
            return false;
        }
        catch (Exception ex)
        {
            log.Error(ex, "Unexpected error during Outlook login");
            return false;
        }
    }

    public static async Task<bool> LoginAsync(EmailAccount account)
    {
        try
        {
            var clientId = CLIENT_ID;
            var scopes = account.Scopes?.Split(',') ?? new[] { "Mail.Read", "User.Read", "offline_access" };

            // Build the MSAL public client app
            IPublicClientApplication app = BuildOutlookApp(clientId);

            // Attempt silent login using the existing account
            var msalAccount = await app.GetAccountAsync(account.AccountId);
            if (msalAccount == null)
            {
                log.Warning($"MSAL account not found for {account.Address}. Needs re-login.");
                return false; // TODO: prompt user to login interactively
            }

            AuthenticationResult result;
            try
            {
                result = await app.AcquireTokenSilent(scopes, msalAccount)
                                  .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                // Silent login failed, interactive login required
                log.Warning($"Silent login failed for {account.Address}. Interactive login required.");
                return false; // TODO: prompt user
            }

            // Update token info
            account.AccessToken = result.AccessToken;
            account.LastAuthTime = DateTime.UtcNow;
            account.TokenExpiry = result.ExpiresOn.UtcDateTime;

            SaveAccount(account);
            log.Info($"Silent login successful for {account.Address}");
            return true;
        }
        catch (MsalException ex)
        {
            log.Error(ex, $"MSAL error during silent login for {account.Address}");
            return false;
        }
        catch (Exception ex)
        {
            log.Error(ex, $"Unexpected error during silent login for {account.Address}");
            return false;
        }
    }

    private static IPublicClientApplication BuildOutlookApp(string clientId)
    {
        var cacheFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinterRoseUtilsApp", "msal_cache.bin");

        var storageProperties = new TokenCachePersistenceOptions
        {
            Name = "msal_cache",
            UnsafeAllowUnencryptedStorage = true // optional, not recommended for production
        };

        var app = PublicClientApplicationBuilder
            .Create(clientId)
            .WithRedirectUri("http://localhost")
            .Build();

        app.UserTokenCache.SetBeforeAccess(args =>
        {
            if (File.Exists(cacheFilePath))
                args.TokenCache.DeserializeMsalV3(File.ReadAllBytes(cacheFilePath));
        });

        app.UserTokenCache.SetAfterAccess(args =>
        {
            if (args.HasStateChanged)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(cacheFilePath)!);
                File.WriteAllBytes(cacheFilePath, args.TokenCache.SerializeMsalV3());
            }
        });
        return app;
    }

    private static void SaveAccount(EmailAccount account)
    {
        lock (typeof(OutlookAuthHandler))
        {
            List<EmailAccount> accounts = Assets.Exists("EmailAccounts")
                ? Assets.Load<List<EmailAccount>>("EmailAccounts")
                : [];

            var existing = accounts.FirstOrDefault(a => a.Address == account.Address);
            if (existing != null)
                accounts.Remove(existing);

            accounts.Add(account);
            Assets.Save("EmailAccounts", accounts);
        }
    }
}
