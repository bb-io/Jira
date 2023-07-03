using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests;

public class PrioritizeIssueRequest
{
    [Display("Issue Key")]
    public string IssueKey { get; set; }
    
    [Display("Priority Id")]
    public string PriorityId { get; set; }
}