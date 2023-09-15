using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Webhooks.Inputs;

public class IssueTypeInput
{
    [Display("Issue type")]
    [DataSource(typeof(IssueTypeDataSourceHandler))]
    public string IssueType { get; set; }
}