using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers;

public class AssigneeIdentifier
{
    [Display("Assignee")]
    [DataSource(typeof(AssigneeDataSourceHandler))]
    public string? AccountId { get; set; }
}