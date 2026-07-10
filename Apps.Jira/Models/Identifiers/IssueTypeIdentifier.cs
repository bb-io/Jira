using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers;

public class IssueTypeIdentifier
{
    [Display("Issue type ID")]
    [DataSource(typeof(IssueTypeDataSourceHandler))]
    public string IssueTypeId { get; set; } = string.Empty;
}
