using System.Text.Json.Serialization;

namespace Apps.Jira.Dtos;

public class AtlassianCloudResourceDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
    
    [JsonPropertyName("scopes")]
    public List<string> Scopes { get; set; }
    
    [JsonPropertyName("avatarUrl")]
    public string AvatarUrl { get; set; }
}