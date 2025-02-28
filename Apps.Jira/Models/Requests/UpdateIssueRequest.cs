using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests;

public class UpdateIssueRequest
{
    [Display("Status (transition) ID")]
    [DataSource(typeof(IssueStatusDataSourceHandler))]
    public string? StatusId { get; set; }
    
    [Display("Assignee account ID")]
    [DataSource(typeof(AssigneeDataSourceHandler))]
    public string? AssigneeAccountId { get; set; }
    
    [Display("Issue type ID")]
    [DataSource(typeof(IssueTypeDataSourceHandler))]
    public string? IssueTypeId { get; set; }
    
    public string? Summary { get; set; }
    
    [Display("Description", Description = "The description of the issue. Expected to be in markdown format but can be plain text.")]
    public string? Description { get; set; }

    [Display("Original Estimate")]
    public string? OriginalEstimate { get; set; }

    [Display("Reporter account ID")]
    [DataSource(typeof(AssigneeDataSourceHandler))]
    public string? Reporter { get; set; }

    [Display("Notify users", Description = "Whether a notification email about the issue update is sent to all watchers. To disable the notification, administer Jira or administer project permissions are required. If the user doesn't have the necessary permission the request is ignored.")]
    public bool? NotifyUsers { get; set; }

    [Display("Override screen security", Description = "Whether screen security is overridden to enable hidden fields to be edited.")]
    public bool? OverrideScreenSecurity { get; set; }

}