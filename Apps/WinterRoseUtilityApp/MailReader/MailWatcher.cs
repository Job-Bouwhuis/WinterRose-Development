using EnvDTE;
using Microsoft.Graph.Models;
using Microsoft.VisualStudio.OLE.Interop;
using Raylib_cs;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Tooltipping;
using WinterRose.Recordium;
using WinterRose.WIP.TestClasses;
using WinterRoseUtilityApp.DrinkReminder;
using WinterRoseUtilityApp.MailReader.Models;
using WinterRoseUtilityApp.MailReader.Readers;
using WinterRoseUtilityApp.MailReader.Watchers;

namespace WinterRoseUtilityApp.MailReader;

public class MailWatcher
{
    public static MailWatcher Current { get; private set; }
    public MulticastVoidInvocation<MailFetchProgress> OnFetchProgress { get; } = new();

    private readonly List<IMailMonitor> monitors = new();
    public IReadOnlyCollection<IMailMonitor> Monitors => monitors.AsReadOnly();
    public TimeSpan checkInterval { get; private set; }
    public int MonitorCount => monitors.Count;
    public DateTime LastCheck { get; private set; } = DateTime.MinValue;
    private bool firstCheckDone = false;

    Log log = new Log("MailWatcher");

    MailPreferences preferences;
    private const string INTERVAL_ASSET_NAME = "MailPreferences";

    public MailWatcher()
    {
        Current = this;
        if (!Assets.Exists(INTERVAL_ASSET_NAME))
            Assets.CreateAsset(INTERVAL_ASSET_NAME);

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

        MailboxStats totals = new MailboxStats();

        IntRef foldersProcessed = new();
        IntRef messagesProcessed = new();
        FetchProgressInfo fetchProgress = new(p => OnFetchProgress.Invoke(p),
                                              new MailFetchProgress(),
                                              totals,
                                              foldersProcessed,
                                              messagesProcessed);

        foreach (IMailMonitor mon in monitors)
        {
            var tots = await mon.FetchStatsAsync();
            fetchProgress.Stats = tots;
        }

        foreach (IMailMonitor monitor in monitors)
        {
            log.Info($"Fetching mail from {monitor.Account.Address}");
            var messages = await monitor.FetchNewAsync(CancellationToken.None, fetchProgress);
            log.Info($"Complete, fetched {messages.Sum(p => p.Value.Count)} mails for {messages.Keys.Count} folders");

            foreach (var (folder, emails) in messages)
            {
                if(emails.Count > 0)
                    log.Info($"Processing {emails.Count} new emails in folder: {folder.DisplayName}");
                foreach (var email in emails)
                {
                    email.OwnerAccount ??= monitor.Account;

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

                    t.Style.TimeUntilAutoDismiss = 15;
                    t.OnToastClicked.Subscribe(Invocation.Create((Toast t, MouseButton m) =>
                    {
                        if(m is MouseButton.Left)
                        {
                            //TODO: make OS invariant
                            WinterRose.Windows.Clipboard.WriteString(email.From);
                            t.Close();
                            var tip = Tooltips.MouseFollow(new System.Numerics.Vector2(200, 90));
                            tip.AddText($"Copied '{email.From}' to clipboard!");
                            tip.Show();
                            tip.Style.TimeUntilAutoDismiss = 2;
                            tip.Style.PauseAutoDismissTimer = false;
                        }
                    }));
                }
            }
        }

        LastCheck = DateTime.UtcNow;
        log.Info("Mail check process completed.");
    }

    public void MarkAsRead(MailMessage email)
    {
        switch (email.OwnerAccount.Provider)
        {
            case "Outlook":
                IMailMonitor outlook = monitors.FirstOrDefault(s => s.Name is "Outlook" && s.Account.Address == email.OwnerAccount.Address) 
                    ?? throw new InvalidOperationException("Could not find mail monitor for Outlook for account " + email.OwnerAccount.Address);
                outlook.MarkAsRead(email.MailFolder, email);
                break;
            default:
                throw new InvalidOperationException($"Unknown email provider '{email.OwnerAccount.Provider}' for '{email.OwnerAccount.Address}'");
        }
    }

    internal void TriggerImmediateCheck()
    {
        if (monitors.Count == 0)
            return;
        LastCheck = DateTime.UtcNow - checkInterval;
        firstCheckDone = true;
    }

    public void UpdateCheckInterval(TimeSpan newInterval)
    {
        if (newInterval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(newInterval), "Interval must be positive.");

        // If no check has ever been done, just set the interval
        if (LastCheck == DateTime.MinValue || LastCheck == DateTime.MinValue + checkInterval)
        {
            // just update interval
            checkInterval = newInterval;
            return;
        }

        // Calculate elapsed time since last start
        TimeSpan elapsedSinceLastCheck = DateTime.UtcNow - (LastCheck == DateTime.MinValue ? DateTime.UtcNow : LastCheck);

        // Update interval
        checkInterval = newInterval;

        // Preserve progress by shifting LastCheck
        LastCheck = DateTime.UtcNow - elapsedSinceLastCheck;
    }

    public async Task<MessageBody> FetchEmailBodyAsync(MailMessage email)
    {
        if (email is null)
            throw new ArgumentNullException(nameof(email));

        // If the body is already loaded, just return it
        if (email.Body != null && !string.IsNullOrWhiteSpace(email.Body.Content))
            return email.Body;

        IMailMonitor? monitor = monitors.FirstOrDefault(m =>
            m.Account.Address == email.OwnerAccount.Address &&
            m.Name == email.OwnerAccount.Provider);

        if (monitor is null)
            throw new InvalidOperationException($"No mail monitor found for account {email.OwnerAccount.Address} ({email.OwnerAccount.Provider})");

        // Only Outlook is implemented right now
        if (monitor is OutlookMailMonitor outlookMonitor)
        {
            // Fetch the full body for this message
            MailMessage fullMessage = await OutlookMailReader.FetchEmailBody(email.OwnerAccount, email);

            // Update the cached message if present
            if (outlookMonitor.GetCached().TryGetValue(email.MailFolder, out var cachedList))
            {
                var cachedMessage = cachedList.FirstOrDefault(m => m.Id == email.Id);
                if (cachedMessage != null)
                    cachedMessage.Body = fullMessage.Body;
            }

            // Update the email itself
            email.Body = fullMessage.Body;

            return fullMessage.Body;
        }

        throw new NotSupportedException($"FetchEmailBodyAsync is not implemented for provider {email.OwnerAccount.Provider}");
    }
}
