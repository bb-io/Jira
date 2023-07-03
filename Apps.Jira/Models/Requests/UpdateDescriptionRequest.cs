using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests;

public class UpdateDescriptionRequest
{
    [Display("Issue Key")]
    public string IssueKey { get; set; }
    
    public string Description { get; set; }
}