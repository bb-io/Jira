using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests;

public class GetIssueCommentsRequest
{
    [Display("Issue key")]
    [DataSource(typeof(IssueDataSourceHandler))]
    public string IssueKey { get; set; }

    [Display("Issue comment IDs"), DataSource(typeof(IssueCommentDataHandler))]
    public IEnumerable<string>? Issues { get; set; }
}
