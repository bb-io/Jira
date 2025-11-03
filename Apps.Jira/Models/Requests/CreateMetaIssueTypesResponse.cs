using Newtonsoft.Json;

namespace Apps.Jira.Models.Requests
{
    public class CreateMetaIssueTypesResponse
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
        public List<CreateMetaIssueType> Values { get; set; }
    }
    public class CreateMetaIssueType
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("subtask")]
        public bool Subtask { get; set; }
    }

    public class CreateMetaFieldsResponse
    {
        [JsonProperty("fields")]
        public Dictionary<string, object> Fields { get; set; }
    }
}
