using Azure.Identity;
using LibreHardwareMonitor.Hardware;
using Microsoft.Graph;
using Newtonsoft.Json;
using System.Net;
using System.Text.Json;
using WinterRose.Recordium;
using WinterRoseUtilityApp.MailReader.AuthHandlers;
using WinterRoseUtilityApp.MailReader.Models;

namespace WinterRoseUtilityApp.MailReader.Readers;

internal static class OutlookMailReader
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

    public static async Task<List<MailMessage>> FetchEmails(EmailAccount account, MailFolder folder, int retries = 0)
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", account.AccessToken);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var response = await http.GetAsync(
                $"{BaseUrl}/mailfolders/{folder.Id}/messages?$top=20&$select=Subject,From,Body,BodyPreview,ReceivedDateTime,isRead&$orderby=ReceivedDateTime desc");
            response.EnsureSuccessStatusCode();

            List<MailMessage> messages = [];

            var json = await response.Content.ReadAsStringAsync();
            var messagesResult = System.Text.Json.JsonSerializer.Deserialize<MailMessagesResponse>(json, options);

            messages.AddRange(messagesResult.Value);

            while (!string.IsNullOrWhiteSpace(messagesResult.ODataNextLink))
            {
                response = await http.GetAsync(messagesResult.ODataNextLink);
                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
                messagesResult = System.Text.Json.JsonSerializer.Deserialize<MailMessagesResponse>(json, options);

                messages.AddRange(messagesResult.Value);
            }

            return messages;
        }
        catch (HttpRequestException ex) when (retries == 0 && ex.Message.Contains("401"))
        {
            log.Warning("Access token expired, attempting silent re-login...");
            bool relogged = await OutlookAuthHandler.LoginAsync(account);
            if (!relogged)
            {
                log.Error("Re-login failed. Aborting folder fetch.");
                return [];
            }

            return await FetchEmails(account, folder, 1);
        }
        catch (Exception ex)
        {
            log.Error(ex);
            return [];
        }
    }


    internal static async Task<bool> ToggleEmailReadStatus(EmailAccount account, MailFolder folder, MailMessage message, bool? markAsRead = null)
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", account.AccessToken);

            bool newReadState = markAsRead ?? !message.IsRead;

            var patchContent = new StringContent(
                JsonConvert.SerializeObject(new { isRead = newReadState }),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var patchResponse = await http.PatchAsync($"{BaseUrl}/mailfolders/{folder.Id}/messages/{message.Id}", patchContent);
            if(patchResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                log.Warning("Rate limited by Microsoft Graph API. Waiting 5 seconds before retrying...");
                return true;
            }
            patchResponse.EnsureSuccessStatusCode();

            message.IsRead = newReadState;

            string sub = message.Subject.Length > 20 ? message.Subject[20..] + "..." : message.Subject;
            log.Info($"Email from {message.From} ({sub}) read status set to {newReadState}");
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
        return false;
    }


}
