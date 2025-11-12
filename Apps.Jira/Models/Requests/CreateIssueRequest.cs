using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests;

public class CreateIssueRequest
{
    public string Summary { get; set; }

    [Display("Issue type ID")]
    [DataSource(typeof(IssueTypeDataSourceHandler))]
    public string IssueTypeId { get; set; }
        
    public string? Description { get; set; }
        
    [Display("Assignee account ID")]
    [DataSource(typeof(AssigneeDataSourceHandler))]
    public string? AccountId { get; set; }

    [Display("Due date")]
    public DateTime? DueDate { get; set; }

    [Display("Original estimate", Description ="Original estimate time in minutes")]
    public string? OriginalEstimate { get; set; }

    [Display("Reporter  ID")]
    [DataSource(typeof(AssigneeDataSourceHandler))]
    public string? Reporter { get; set; }

    [Display("Parent issue key")]
    [DataSource(typeof(IssueDataSourceHandler))]
    public string? ParentIssueKey { get; set; }

    [Display("Labels", Description = "Optional list of labels (Jira labels are strings).")]
    [DataSource(typeof(IssueLabelDataHandler))]
    public IEnumerable<string>? Labels { get; set; }
}