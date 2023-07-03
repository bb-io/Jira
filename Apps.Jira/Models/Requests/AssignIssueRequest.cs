using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests
{
    public class AssignIssueRequest
    {
        [Display("Issue Key")]
        public string IssueKey { get; set; }
        
        [Display("Account Id")]
        public string? AccountId { get; set; }
    }
}