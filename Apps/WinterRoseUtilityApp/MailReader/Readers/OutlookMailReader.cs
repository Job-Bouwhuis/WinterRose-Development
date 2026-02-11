using Azure.Identity;
using LibreHardwareMonitor.Hardware;
using Microsoft.Graph;
using Newtonsoft.Json;
using System.Net;
using System.Text.Json;
using WinterRose.EventBusses;
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

    public static async Task<List<MailMessage>> FetchEmails(
        EmailAccount account,
        MailFolder folder,
        FetchProgressInfo? fetchInfo = null,
        bool fetchBody = false,
        int retries = 0)
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

            // Only request Body if fetchBody == true
            string selectFields = fetchBody
                ? "Subject,From,Body,BodyPreview,ReceivedDateTime,isRead"
                : "Subject,From,BodyPreview,ReceivedDateTime,isRead";

            var response = await http.GetAsync(
                $"{BaseUrl}/mailfolders/{folder.Id}/messages?$top=20&$select={selectFields}&$orderby=ReceivedDateTime desc");
            response.EnsureSuccessStatusCode();

            List<MailMessage> messages = [];

            var json = await response.Content.ReadAsStringAsync();
            var messagesResult = System.Text.Json.JsonSerializer.Deserialize<MailMessagesResponse>(json, options);

            foreach (var message in messagesResult.Value)
            {
                message.OwnerAccount = account;
                message.MailFolder = folder;
                messages.Add(message);
            }

            fetchInfo?.ProcessedMessages.Value += messagesResult.Value.Count;
            fetchInfo?.CurrentFolderName = folder.DisplayName;
            fetchInfo?.InvokeNow($"Fetching mails in '{folder.DisplayName}'");

            while (!string.IsNullOrWhiteSpace(messagesResult.ODataNextLink))
            {
                response = await http.GetAsync(messagesResult.ODataNextLink);
                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
                messagesResult = System.Text.Json.JsonSerializer.Deserialize<MailMessagesResponse>(json, options);

                messages.AddRange(messagesResult.Value);
                fetchInfo?.ProcessedMessages.Value += messagesResult.Value.Count;
                fetchInfo?.InvokeNow($"Fetching mails in '{folder.DisplayName}'");
            }

            return messages;
        }
        catch (HttpRequestException ex) when (retries == 0 && ex.Message.Contains("401"))
        {
            log.Warning("Access token expired, attempting silent re-login...");
            bool relogged = await OutlookAuthHandler.LoginAsync(account);
            if (!relogged)
            {
                log.Error("Re-login failed. Aborting email fetch.");
                return [];
            }

            return await FetchEmails(account, folder, fetchInfo, fetchBody, 1);
        }
        catch (Exception ex)
        {
            log.Error(ex);
            return [];
        }
    }

    /// <summary>
    /// Fetch the full body for a single email message.
    /// </summary>
    public static async Task<MailMessage> FetchEmailBody(EmailAccount account, MailMessage message, int retries = 0)
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
                $"{BaseUrl}/mailfolders/{message.MailFolder.Id}/messages/{message.Id}?$select=Body,BodyPreview");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var fullMessage = System.Text.Json.JsonSerializer.Deserialize<MailMessage>(json, options);

            fullMessage.OwnerAccount = account;
            fullMessage.MailFolder = message.MailFolder;

            return fullMessage;
        }
        catch (HttpRequestException ex) when (retries == 0 && ex.Message.Contains("401"))
        {
            log.Warning("Access token expired, attempting silent re-login...");
            bool relogged = await OutlookAuthHandler.LoginAsync(account);
            if (!relogged)
            {
                log.Error("Re-login failed. Aborting fetch of email body.");
                return message;
            }

            return await FetchEmailBody(account, message, 1);
        }
        catch (Exception ex)
        {
            log.Error(ex);
            return message;
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

    public static async Task<MailboxStats> FetchMailboxStatsAsync(EmailAccount account, int retries = 0)
    {
        try
        {
            using var http = new HttpClient();

            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", account.AccessToken);

            string requestUrl =
                $"{BaseUrl}/mailfolders?$select=id,totalItemCount,unreadItemCount";

            int folderCount = 0;
            int totalMessages = 0;
            int totalUnread = 0;

            while (!string.IsNullOrWhiteSpace(requestUrl))
            {
                var response = await http.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var folderResult = JsonConvert.DeserializeObject<MailFolderResponse>(json);

                foreach (var folder in folderResult.Folders)
                {
                    folderCount++;
                    totalMessages += folder.TotalItemCount;
                    totalUnread += folder.UnreadItemCount;
                }

                requestUrl = folderResult.ODataNextLink;
            }

            return new MailboxStats
            {
                FolderCount = folderCount,
                TotalMessageCount = totalMessages,
                TotalUnreadCount = totalUnread
            };
        }
        catch (HttpRequestException ex) when (retries == 0 && ex.Message.Contains("401"))
        {
            log.Warning("Access token expired, attempting silent re-login...");

            bool relogged = await OutlookAuthHandler.LoginAsync(account);
            if (!relogged)
            {
                log.Error("Re-login failed. Aborting mailbox stats fetch.");
                return new MailboxStats();
            }

            return await FetchMailboxStatsAsync(account, 1);
        }
        catch (Exception ex)
        {
            log.Error(ex);
            return new MailboxStats();
        }
    }
}
