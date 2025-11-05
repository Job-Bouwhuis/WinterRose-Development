using Azure.Identity;
using Microsoft.Graph;
using Newtonsoft.Json;
using WinterRose.Recordium;
using WinterRoseUtilityApp.MailReader.AuthHandlers;
using WinterRoseUtilityApp.MailReader.Models;

namespace WinterRoseUtilityApp.MailReader;

internal static class MailReader
{
    private const string BaseUrl = "https://graph.microsoft.com/v1.0/me";

    static Log log = new Log("Mail Reader");

    public static async Task<List<MailFolder>> FetchFolders(EmailAccount account)
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", account.AccessToken);

            var response = await http.GetAsync($"{BaseUrl}/mailfolders");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var folderResult = JsonConvert.DeserializeObject<MailFolderResponse>(json);
            return folderResult.Folders;
        }
        catch (Exception e)
        {
            log.Error(e);
            return [];
        }
    }

    public static async Task<List<MailMessage>> FetchEmails(EmailAccount account, string folderId)
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", account.AccessToken);

            var response = await http.GetAsync(
    $"{BaseUrl}/mailfolders/{folderId}/messages?$top=20&$select=Subject,From,BodyPreview,ReceivedDateTime,isRead&$orderby=ReceivedDateTime desc");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var messagesResult = JsonConvert.DeserializeObject<MailMessagesResponse>(json);

            return messagesResult.Value;
        }
        catch (Exception ex)
        {
            log.Error(ex);
            return [];
        }
    }
}
