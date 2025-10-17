using Newtonsoft.Json;

namespace Apps.Jira.Webhooks.Responses
{
    public class SimpleStatusDto
    {
        [JsonProperty("name")]
        public string Name { get; set; } = default!;
    }
}
