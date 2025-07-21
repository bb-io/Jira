using Newtonsoft.Json;

namespace Apps.Jira.Dtos;

public class FixVersionDto
{
    [JsonProperty("id")]
    public string VersionId { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string VersionName { get; set; } = string.Empty;

    [JsonProperty("archived")]
    public bool IsVersionArchived { get; set; }

    [JsonProperty("released")]
    public bool IsVersionReleasedArchived { get; set; }
}
