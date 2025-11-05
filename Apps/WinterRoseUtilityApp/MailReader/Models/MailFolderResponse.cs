using Newtonsoft.Json;

namespace WinterRoseUtilityApp.MailReader.Models;

public class MailFolderResponse
{
    [JsonProperty("@odata.context")]
    public string ODataContext { get; set; }

    [JsonProperty("@odata.nextLink")]
    public string ODataNextLink { get; set; }

    [JsonProperty("value")]
    public List<MailFolder> Folders { get; set; }
}
