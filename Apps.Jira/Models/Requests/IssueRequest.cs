using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests
{
    public class IssueRequest
    {
        [Display("Issue key")]
        public string IssueKey { get; set; }
    }
}
