using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Webhooks.Responses
{
    public class IssuesReachedStatusResponse
    {
        [Display("Issues")]
        public List<IssueResponse> Issues { get; set; } = new();
    }
}
