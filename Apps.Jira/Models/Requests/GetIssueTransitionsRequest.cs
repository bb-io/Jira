using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests
{
    public class GetIssueTransitionsRequest
    {
        [Display("Issue key")]
        public string IssueKey { get; set; }
    }
}
