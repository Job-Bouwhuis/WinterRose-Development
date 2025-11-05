using System.Text.Json.Serialization;

namespace WinterRoseUtilityApp.MailReader.Models;

public class MailMessagesResponse
{
    [JsonPropertyName("@odata.context")]
    public string ODataContext { get; set; }

    [JsonPropertyName("value")]
    public List<MailMessage> Value { get; set; }

    [JsonPropertyName("@odata.nextLink")]
    public string ODataNextLink { get; set; }
}
