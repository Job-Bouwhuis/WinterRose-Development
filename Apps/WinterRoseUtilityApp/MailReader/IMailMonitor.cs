using Microsoft.Identity.Client;
using WinterRose.EventBusses;
using WinterRoseUtilityApp.MailReader.Models;
using static WinterRoseUtilityApp.MailReader.Readers.OutlookMailReader;

namespace WinterRoseUtilityApp.MailReader;

public interface IMailMonitor
{
    string Name { get; }
    EmailAccount Account { get; }
    Task<Dictionary<MailFolder, List<MailMessage>>> FetchNewAsync(CancellationToken ct, FetchProgressInfo progress);
    Task<MailboxStats> FetchStatsAsync();
    Dictionary<MailFolder, List<MailMessage>> GetCached();
    bool MarkAsRead(MailFolder folder, MailMessage message);
    Task FetchBodyAsync(MailMessage message);
}
