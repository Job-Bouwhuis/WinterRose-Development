using Microsoft.Identity.Client;
using WinterRoseUtilityApp.MailReader.Models;
using static WinterRoseUtilityApp.MailReader.MailReader;

namespace WinterRoseUtilityApp.MailReader;

public interface IMailMonitor
{
    EmailAccount Account { get; }
    Task<List<MailMessage>> FetchNewAsync(CancellationToken ct);
}
