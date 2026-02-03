using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests;

public class SetResolutionRequest
{
    [Display("Resolution")]
    [DataSource(typeof(IssueResolutionDataSourceHandler))]
    public string ResolutionId { get; set; } = default!;
}
