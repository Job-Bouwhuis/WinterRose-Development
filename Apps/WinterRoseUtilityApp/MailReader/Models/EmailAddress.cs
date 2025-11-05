using System.Text.Json.Serialization;

namespace WinterRoseUtilityApp.MailReader.Models;

public class EmailAddress
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }

    public override string ToString() => Address;
    public static implicit operator string(EmailAddress sender) => sender.ToString();
}
