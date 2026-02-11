using System.Text.Json.Serialization;

namespace WinterRoseUtilityApp.MailReader.Models;

public class MessageBody
{
    [JsonPropertyName("contentType")]
    public string ContentType { get; set; } // "text" or "html"

    [JsonPropertyName("content")]
    public string Content { get; set; }

    public bool Ready { get; set; } = true;
}