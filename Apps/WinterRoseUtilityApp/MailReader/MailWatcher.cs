using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using Raylib_cs;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.Recordium;
using WinterRose.WIP.TestClasses;
using WinterRoseUtilityApp.DrinkReminder;
using WinterRoseUtilityApp.MailReader.Models;

namespace WinterRoseUtilityApp.MailReader;

public class MailWatcher
{
    private readonly List<IMailMonitor> monitors = new();
    public readonly TimeSpan checkInterval;
    public DateTime LastCheck { get; private set; } = DateTime.MinValue;
    private bool firstCheckDone = false;

    Log log = new Log("MailWatcher");

    MailPreferences preferences;
    private const string INTERVAL_ASSET_NAME = "MailPreferences";

    public MailWatcher()
    {
        if (!Assets.Exists(INTERVAL_ASSET_NAME))
        {
            Assets.CreateAsset(INTERVAL_ASSET_NAME);
        }

        preferences = Assets.Load<MailPreferences>(INTERVAL_ASSET_NAME);
        checkInterval = TimeSpan.FromMinutes(preferences.CheckIntervalMinutes);
    }

    public void AddMonitor(IMailMonitor monitor)
    {
        monitors.Add(monitor);
    }

    public async Task UpdateAsync()
    {
        if (!firstCheckDone)
        {
            if (monitors.Count == 0)
                return;
            firstCheckDone = true;
            log.Info("Forcing initial mail check...");
            LastCheck = DateTime.UtcNow - checkInterval;
        }

        if (LastCheck == DateTime.MinValue)
            return; // a check is still busy
        if (DateTime.UtcNow - LastCheck < checkInterval)
            return;

        LastCheck = DateTime.MinValue;

        log.Info("Mail check process started...");

        foreach (IMailMonitor monitor in monitors)
        {
            log.Info($"Fetching mail from {monitor.Account.Address}");
            var messages = await monitor.FetchNewAsync(CancellationToken.None);
            log.Info($"Complete, fetched {messages.Sum(p => p.Value.Count)} mails for {messages.Keys.Count} folders");

            foreach (var (folder, emails) in messages)
            {
                if(emails.Count > 0)
                    log.Info($"Processing {emails.Count} new emails in folder: {folder.DisplayName}");
                foreach (var email in emails)
                {
                    if (email.From is null)
                        continue;

                    if (email.IsRead)
                        continue;

                    if (preferences.IgnoredSenders.Contains(email.From))
                    {
                        if(monitor.MarkAsRead(folder, email))
                        {
                            log.Warning("Mailwatcher rate limited by external API. waiting 5 seconds before proceeding to the next mail...");
                            await Task.Delay(5000);
                            log.Info("Resuming mail processing...");
                        }
                        continue;
                    }

                    Toast t;
                    if (preferences.ImportantSenders.Contains(email.From))
                        t = Toasts.Success($"Important mail from {email.From}: {email.Subject}");
                    else
                        t = Toasts.Neutral($"\\c[#00FF00]New mail from \\c[#00FFFF]{email.From}\\c[white]: {email.Subject}");

                    log.Debug(email.From);

                    t.Style.TimeUntilAutoDismiss = 15;
                    t.OnToastClicked.Subscribe(Invocation.Create((Toast t, MouseButton m) =>
                    {
                        if(m is MouseButton.Left)
                        {
                            //TODO: make OS invariant
                            WinterRose.Windows.Clipboard.WriteString(email.From);
                            t.Close();
                        }
                    }));
                }
            }
        }

        LastCheck = DateTime.UtcNow;
        log.Info("Mail check process completed.");
    }
}
