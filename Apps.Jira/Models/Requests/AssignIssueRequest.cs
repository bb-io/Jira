using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests
{
    public class AssignIssueRequest
    {
        [Display("Issue key")]
        public string IssueKey { get; set; }
        
        [Display("Account ID")]
        public string? AccountId { get; set; }
    }
}