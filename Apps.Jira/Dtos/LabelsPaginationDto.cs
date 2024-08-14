using Newtonsoft.Json;

namespace Apps.Jira.Dtos;

public class LabelsPaginationDto
{
    [JsonProperty("maxResults")]
    public int MaxResults { get; set; }
    
    [JsonProperty("startAt")]
    public int StartAt { get; set; }
    
    [JsonProperty("total")]
    public int Total { get; set; }
    
    [JsonProperty("isLast")]
    public bool IsLast { get; set; }
    
    [JsonProperty("values")]
    public List<string> Values { get; set; } = new();
}