using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.Jira.Dtos;

public class ErrorDto
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("errorMessages")]
    public IEnumerable<string> ErrorMessages { get; set; }

    [JsonProperty("errors")]
    public JObject Errors { get; set; }
}