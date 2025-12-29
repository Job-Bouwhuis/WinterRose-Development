using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRoseUtilityApp.MailReader.Models;
using WinterRoseUtilityApp.MailReader.Readers;

namespace WinterRoseUtilityApp.MailReader.Watchers;

public class OutlookMailMonitor : IMailMonitor
{
    public EmailAccount Account { get; }

    private readonly Dictionary<string, HashSet<string>> seenMessageIds = new();

    public OutlookMailMonitor(EmailAccount account) => Account = account;

    public Task<Dictionary<MailFolder, List<MailMessage>>> FetchNewAsync(CancellationToken ct)
    {
        return Task.Run(async () =>
        {
            Dictionary<MailFolder, List<MailMessage>> result = [];

            // Fetch folders (Inbox, etc.)
            var folders = await OutlookMailReader.FetchFolders(Account);
            foreach (var folder in folders)
            {
                List<MailMessage> mail = [];
                var folderId = folder.Id;
                if (!seenMessageIds.ContainsKey(folderId))
                    seenMessageIds[folderId] = new HashSet<string>();

                var lastSeen = seenMessageIds[folderId];

                var messages = await OutlookMailReader.FetchEmails(Account, folder);

                foreach (var msg in messages)
                {
                    if (lastSeen.Contains(msg.Id))
                        continue; // already processed

                    lastSeen.Add(msg.Id);
                    mail.Add(msg);
                }
                result[folder] = mail;
            }

            return result;
        });
    }

    public bool MarkAsRead(MailFolder folder, MailMessage message)
    {
        if (message.IsRead)
            return false;

        bool tooManyRequests = ForgeWardenEngine.Current.GlobalThreadLoom.ComputeOn(
            ForgeWardenEngine.ENGINE_POOL_NAME, OutlookMailReader.ToggleEmailReadStatus(Account, folder, message, true));
        return tooManyRequests;
    }
}
