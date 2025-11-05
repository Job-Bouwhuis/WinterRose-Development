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

    public static async Task<List<MailFolder>> FetchFolders(EmailAccount account, int retries = 0)
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", account.AccessToken);

            var response = await http.GetAsync($"{BaseUrl}/mailfolders");
            response.EnsureSuccessStatusCode();

            List<MailFolder> folders = [];

            var json = await response.Content.ReadAsStringAsync();
            var folderResult = JsonConvert.DeserializeObject<MailFolderResponse>(json);

            folders.AddRange(folderResult.Folders);

            while(!string.IsNullOrWhiteSpace(folderResult.ODataNextLink))
            {
                response = await http.GetAsync(folderResult.ODataNextLink);
                response.EnsureSuccessStatusCode();
                json = await response.Content.ReadAsStringAsync();
                folderResult = JsonConvert.DeserializeObject<MailFolderResponse>(json);

                folders.AddRange(folderResult.Folders);
            }

            return folders;
        }
        catch (HttpRequestException ex) when (retries == 0 && ex.Message.Contains("401 (Unauthorized)"))
        {
            log.Warning("Access token expired, attempting silent re-login...");
            bool relogged = await OutlookAuthHandler.LoginAsync(account);
            if (!relogged)
            {
                log.Error("Re-login failed. Aborting folder fetch.");
                return [];
            }

            return await FetchFolders(account, 1);
        }
        catch (Exception e)
        {
            log.Error(e);
            return [];
        }
    }

    public static async Task<List<MailMessage>> FetchEmails(EmailAccount account, string folderId, int retries = 0)
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
        catch (HttpRequestException ex) when (retries == 0 && ex.Message.Contains("401 (Unauthorized)"))
        {
            log.Warning("Access token expired, attempting silent re-login...");
            bool relogged = await OutlookAuthHandler.LoginAsync(account);
            if (!relogged)
            {
                log.Error("Re-login failed. Aborting folder fetch.");
                return [];
            }

            return await FetchEmails(account, folderId, 1);
        }
        catch (Exception ex)
        {
            log.Error(ex);
            return [];
        }
    }
}
