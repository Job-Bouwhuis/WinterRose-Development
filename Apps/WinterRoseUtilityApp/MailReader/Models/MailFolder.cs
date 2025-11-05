using Newtonsoft.Json;

namespace WinterRoseUtilityApp.MailReader.Models;

public class MailFolder
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("displayName")]
    public string DisplayName { get; set; }

    [JsonProperty("parentFolderId")]
    public string ParentFolderId { get; set; }

    [JsonProperty("childFolderCount")]
    public int ChildFolderCount { get; set; }

    [JsonProperty("unreadItemCount")]
    public int UnreadItemCount { get; set; }

    [JsonProperty("totalItemCount")]
    public int TotalItemCount { get; set; }

    [JsonProperty("isHidden")]
    public bool IsHidden { get; set; }
}
