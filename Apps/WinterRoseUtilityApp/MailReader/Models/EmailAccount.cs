using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRoseUtilityApp.MailReader.Models;
public class EmailAccount
{
    public string Provider { get; set; }        // e.g., "Outlook"
    public string Address { get;set; }         // Email address
    public string DisplayName { get; set; }     // Friendly name for UI

    public string AccessToken { get; set; }     // Current access token for Graph API
    public DateTime LastAuthTime { get; set; }  // When this token was last acquired
    public DateTime TokenExpiry { get; set; }   // Expiry for current token

    public string TenantId { get;  set; }        // Tenant the account belongs to
    public string Scopes { get; set; }          // Comma-separated scopes granted
    public string AccountId { get; set; }       // MSAL Account identifier

    public IAccount Account { get; set; }

    public EmailAccount(
        string provider,
        string address,
        string displayName,
        string accessToken,
        DateTime lastAuthTime,
        DateTime tokenExpiry,
        string tenantId = null,
        string scopes = null,
        string accountId = null)
    {
        Provider = provider;
        Address = address;
        DisplayName = displayName;
        AccessToken = accessToken;
        LastAuthTime = lastAuthTime;
        TokenExpiry = tokenExpiry;
        TenantId = tenantId;
        Scopes = scopes;
        AccountId = accountId;
    }

    private EmailAccount() { }

    public bool IsTokenExpired =>
        DateTime.UtcNow >= TokenExpiry.AddMinutes(-5); // small buffer to preempt expiry

    public override string ToString() =>
        $"{DisplayName} ({Address}) [{Provider}] Expires: {TokenExpiry:u}";
}

