using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos;

public class CreatedIssueDto
{
    [Display("Issue key")]
    public string Key { get; set; }
    
    [Display("Created issue URL")]
    public string Self { get; set; }

    [Display("Project key")]
    public string? ProjectKey { get; set; }

    [Display("Project ID")]
    public string? ProjectId { get; set; }

    [Display("Project name")]
    public string? ProjectName { get; set; }

    [Display("Issue type ID")]
    public string? IssueTypeId { get; set; }

    [Display("Issue type name")]
    public string? IssueTypeName { get; set; }
}
