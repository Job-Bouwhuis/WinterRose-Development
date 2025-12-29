using Microsoft.Identity.Client;
using WinterRoseUtilityApp.MailReader.Models;
using static WinterRoseUtilityApp.MailReader.Readers.OutlookMailReader;

namespace WinterRoseUtilityApp.MailReader;

public interface IMailMonitor
{
    EmailAccount Account { get; }
    Task<Dictionary<MailFolder, List<MailMessage>>> FetchNewAsync(CancellationToken ct);
    bool MarkAsRead(MailFolder folder, MailMessage message);
}
