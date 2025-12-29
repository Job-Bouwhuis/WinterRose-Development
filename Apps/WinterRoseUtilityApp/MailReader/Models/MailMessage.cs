using System.Text.Json.Serialization;

namespace WinterRoseUtilityApp.MailReader.Models;

public class MailMessage
{
    [JsonPropertyName("@odata.etag")]
    public string ODataETag { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("receivedDateTime")]
    public DateTime ReceivedDateTime { get; set; }

    [JsonPropertyName("subject")]
    public string Subject { get; set; }

    [JsonPropertyName("bodyPreview")]
    public string BodyPreview { get; set; }

    [JsonPropertyName("from")]
    public SenderAddress From { get; set; }

    [JsonPropertyName("isRead")]
    public bool IsRead { get; set; }

    [JsonPropertyName("body")]
    public MessageBody Body { get; set; }
}
