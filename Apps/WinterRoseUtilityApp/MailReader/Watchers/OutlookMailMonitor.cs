using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRoseUtilityApp.MailReader.Models;
using WinterRoseUtilityApp.MailReader.Readers;

namespace WinterRoseUtilityApp.MailReader.Watchers;

public class OutlookMailMonitor : IMailMonitor
{
    public EmailAccount Account { get; }

    public string Name => "Outlook";

    public MulticastVoidInvocation<MailFetchProgress> OnFetchProgress { get; } = new();

    private readonly Dictionary<string, HashSet<string>> seenMessageIds = new();
    private readonly Dictionary<MailFolder, List<MailMessage>> cachedMessages = new();

    public OutlookMailMonitor(EmailAccount account) => Account = account;

    public Task<Dictionary<MailFolder, List<MailMessage>>> FetchNewAsync(CancellationToken ct, FetchProgressInfo progress)
    {
        return Task.Run(async () =>
        {
            Dictionary<MailFolder, List<MailMessage>> result = [];

            var totals = await OutlookMailReader.FetchMailboxStatsAsync(Account);

            var folders = await OutlookMailReader.FetchFolders(Account);

            progress.InvokeNow($"Fetched folders for {Account.Address}");

            foreach (var folder in folders)
            {
                List<MailMessage> mail = [];
                var folderId = folder.Id;
                if (!seenMessageIds.ContainsKey(folderId))
                    seenMessageIds[folderId] = new HashSet<string>();

                var lastSeen = seenMessageIds[folderId];

                // **Fetch metadata only (no body)**
                var messages = await OutlookMailReader.FetchEmails(Account, folder, progress, fetchBody: false);

                foreach (var msg in messages)
                {
                    if (lastSeen.Contains(msg.Id))
                        continue;

                    lastSeen.Add(msg.Id);
                    mail.Add(msg);

                    msg.OwnerAccount ??= Account;
                }

                result[folder] = mail;
                cachedMessages[folder] = mail;

                progress.ProcessedFolders.Value++;
                progress.InvokeNow($"Finished folder '{folder.DisplayName}'");
            }

            progress.InvokeNow($"Finished fetching emails for {Account.Address}");

            return result;
        });
    }

    public bool MarkAsRead(MailFolder folder, MailMessage message)
    {
        if (message.IsRead)
            return false;

        bool tooManyRequests = ForgeWardenEngine.Current.GlobalThreadLoom.ComputeOn(
            ForgeWardenEngine.ENGINE_POOL_NAME, OutlookMailReader.ToggleEmailReadStatus(Account, folder, message, true));

        if (!tooManyRequests)
        {
            if (cachedMessages.TryGetValue(folder, out var cachedList))
            {
                var cachedMessage = cachedList.FirstOrDefault(m => m.Id == message.Id);
                if (cachedMessage != null)
                    cachedMessage.IsRead = true;
            }

            message.IsRead = true;
        }

        return tooManyRequests;
    }

    public Task<MailboxStats> FetchStatsAsync() => OutlookMailReader.FetchMailboxStatsAsync(Account);

    public Dictionary<MailFolder, List<MailMessage>> GetCached() => cachedMessages;

    /// <summary>
    /// Fetches the full body content for a specific email and updates the cache.
    /// </summary>
    public async Task FetchBodyAsync(MailMessage message)
    {
        if (message.Body?.Content != null)
            return; // already loaded

        var fullMessage = await OutlookMailReader.FetchEmailBody(Account, message);

        // Update cached message
        foreach (var folder in cachedMessages.Keys)
        {
            var cachedList = cachedMessages[folder];
            var cachedMessage = cachedList.FirstOrDefault(m => m.Id == message.Id);
            if (cachedMessage != null)
            {
                cachedMessage.Body = fullMessage.Body;
                break;
            }
        }

        // Also update the original message reference
        message.Body = fullMessage.Body;
    }
}

