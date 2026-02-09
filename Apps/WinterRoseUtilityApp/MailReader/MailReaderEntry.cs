using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.Utility;
using WinterRose.Recordium;
using WinterRoseUtilityApp.MailReader.Watchers;
using WinterRoseUtilityApp.SubSystems;

namespace WinterRoseUtilityApp.MailReader;

internal class MailReaderEntry : SubSystem
{
    private const string EmailAccountsHotkey = "EmailAccountsHotkey";

    MailWatcher watcher = new MailWatcher();

    Task? updateTask;

    public MailReaderEntry() :
        base("Mail Reader", "Reads mail and gives notifications for them", new Version(1, 0, 0))
    {
        GlobalHotkey.RegisterHotkey(
            EmailAccountsHotkey, 
            true, 
            HotkeyScancode.LeftAlt, HotkeyScancode.LeftShift, HotkeyScancode.E); // Left Alt + Shift + E

    }

    public override void Init()
    {
        Task.Run(ReadKnownEmails).ContinueWith(SetupReader);
    }

    private void SetupReader(Task task)
    {
        var accounts = EmailAuthManager.GetSavedAccounts();
        foreach (var account in accounts)
        {
            var outlookMonitor = new OutlookMailMonitor(account);
            watcher.AddMonitor(outlookMonitor);
        }
    }

    async void ReadKnownEmails()
    {
        var accounts = EmailAuthManager.GetSavedAccounts();

        if (accounts.Count == 0)
        {
            log.Info("No saved email accounts found. Mail watcher will remain inactive.");
            return;
        }

        foreach (var account in accounts)
        {
            try
            {
                bool loggedIn = await EmailAuthManager.TryLoginAsync(account.Provider, account.Address);
                if (loggedIn)
                {
                    log.Info($"Successfully authenticated {account.Provider} account '{account.Address}'.");
                }
                else
                {
                    log.Warning($"Failed to silently authenticate {account.Provider} account '{account.Address}'.");
                    // TODO: mark for re-login next time or prompt user
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, $"Error while logging in {account.Provider} account '{account.Address}'.");
            }
        }
    }

    public override void Update()
    {
        if (GlobalHotkey.IsTriggered(EmailAccountsHotkey))
        {
            ContainerCreators.EmailAccountsWindow(watcher).Show();
        }

        if (updateTask != null)
            return;
        updateTask = watcher.UpdateAsync();
        updateTask.ContinueWith((t) => updateTask = null);
    }
}
