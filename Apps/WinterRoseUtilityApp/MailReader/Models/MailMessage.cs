using System.Reflection.Metadata;
using System.Text.Json.Serialization;
using WinterRose.ForgeWarden;

namespace WinterRoseUtilityApp.MailReader.Models;

public class MailMessage
{
    [JsonIgnore]
    public EmailAccount OwnerAccount
    {
        get;
        set;
    }

    [JsonIgnore]
    public MailFolder MailFolder { get; set; }

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

    [JsonIgnore]
    private bool fetchingBody = false;

    [JsonIgnore]
    public Action<MessageBody> OnBodyReady { get; set; }

    [JsonPropertyName("body")]
    public MessageBody Body
    {
        get
        {
            if (field is null && !fetchingBody)
            {
                fetchingBody = true;
                field = new MessageBody();
                field.Ready = false;

                ForgeWardenEngine.Current.GlobalThreadLoom
                    .InvokeOn(ForgeWardenEngine.ENGINE_POOL_NAME, async void () =>
                {
                    MessageBody body = await MailWatcher.Current.FetchEmailBodyAsync(this);
                    field = body;
                    field.Ready = true;
                    OnBodyReady?.Invoke(field);
                });
            }

            return field ?? new MessageBody { Content = BodyPreview, ContentType = "text", Ready = true };
        }
        set;
    }
}
