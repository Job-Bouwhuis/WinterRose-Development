using System.Text.Json.Serialization;

namespace WinterRoseUtilityApp.MailReader.Models;

public class SenderAddress
{
    [JsonPropertyName("emailAddress")]
    public EmailAddress EmailAddress { get; set; }

    public override string ToString() => EmailAddress.ToString();
    public static implicit operator string(SenderAddress sender) => sender.ToString();
}
