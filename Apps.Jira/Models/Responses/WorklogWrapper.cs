using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Jira.Models.Responses
{
    public class WorklogWrapper
    {
        [JsonProperty("startAt"), DefinitionIgnore]
        [Display("Start at")]
        public int StartAt { get; set; }

        [JsonProperty("maxResults"), DefinitionIgnore]
        [Display("Max results")]
        public int MaxResults { get; set; }

        [JsonProperty("total")]
        [Display("Total")]
        public int Total { get; set; }

        [JsonProperty("worklogs")]
        [Display("Worklogs")]
        public List<WorklogDto> Worklogs { get; set; } = new();
    }

    public class WorklogDto
    {
        [JsonProperty("id")]
        [Display("Worklog ID")]
        public string Id { get; set; } = default!;

        [JsonProperty("issueId")]
        [Display("Issue ID")]
        public string IssueId { get; set; } = default!;

        [JsonProperty("created")]
        [Display("Created")]
        public DateTime Created { get; set; }

        [JsonProperty("updated")]
        [Display("Updated")]
        public DateTime Updated { get; set; }

        [JsonProperty("started")]
        [Display("Started")]
        public DateTime Started { get; set; }

        [JsonProperty("timeSpent")]
        [Display("Time spent")]
        public string TimeSpent { get; set; } = default!;

        [JsonProperty("timeSpentSeconds")]
        [Display("Time spent (in seconds)")]
        public int TimeSpentSeconds { get; set; }

        [JsonProperty("author")]
        [Display("Worklog author")]
        public UserDto? Author { get; set; }
    }
}
