using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests;

public class UpdateIssueSummaryRequest
{
    [Display("Issue key")]
    public string IssueKey { get; set; }
    
    public string Summary { get; set; }
}