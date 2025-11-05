using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRoseUtilityApp.MailReader.Models;

namespace WinterRoseUtilityApp.MailReader.Watchers;
public class OutlookMailMonitor : IMailMonitor
{
    public EmailAccount Account { get; }

    // Tracks the last seen message IDs per folder
    private readonly Dictionary<string, HashSet<string>> seenMessageIds = new();

    // Optional: per-monitor sender filters
    public HashSet<string> ImportantSenders { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> IgnoredSenders { get; } = new(StringComparer.OrdinalIgnoreCase);

    public OutlookMailMonitor(EmailAccount account)
    {
        Account = account;
    }

    public Task<List<MailMessage>> FetchNewAsync(CancellationToken ct)
    {
        return Task.Run(async () =>
        {
            var newMessages = new List<MailMessage>();

            // Fetch folders (Inbox, etc.)
            var folders = await MailReader.FetchFolders(Account);
            foreach (var folder in folders)
            {
                var folderId = folder.Id;

                if (!seenMessageIds.ContainsKey(folderId))
                    seenMessageIds[folderId] = new HashSet<string>();

                var lastSeen = seenMessageIds[folderId];

                // Fetch messages for this folder
                var messages = await MailReader.FetchEmails(Account, folderId);

                foreach (var msg in messages)
                {
                    if (lastSeen.Contains(msg.Id))
                        continue; // already processed

                    lastSeen.Add(msg.Id); // mark as seen
                    newMessages.Add(msg);

                    // Show toast based on sender
                    if (msg.From is null)
                        continue;
                    if (IgnoredSenders.Contains(msg.From))
                        continue;

                    if(msg.IsRead)
                        continue;

                    if (ImportantSenders.Contains(msg.From))
                        Toasts.Success($"Important mail from {msg.From}: {msg.Subject}").Style.TimeUntilAutoDismiss = 15;
                    else
                        Toasts.Neutral($"\\c[#00FF00]New mail from \\c[#00FFFF]{msg.From}\\c[white]: {msg.Subject}").Style.TimeUntilAutoDismiss = 15;
                }
            }

            return newMessages;
        });
    }
}
