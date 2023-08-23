using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos;

public class CreatedIssueDto
{
    [Display("Issue")]
    public string Key { get; set; }
    
    [Display("Created issue URL")]
    public string Self { get; set; }
}