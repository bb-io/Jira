using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Webhooks.Inputs;

public class IssueInput
{
    [Display("Issue")]
    [DataSource(typeof(IssueDataSourceHandler))]
    public string? IssueKey { get; set; }
}