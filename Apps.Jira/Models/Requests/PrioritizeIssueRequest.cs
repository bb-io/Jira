using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests;

public class PrioritizeIssueRequest
{
    [Display("Issue key")]
    public string IssueKey { get; set; }
    
    [Display("Priority ID")]
    public string PriorityId { get; set; }
}