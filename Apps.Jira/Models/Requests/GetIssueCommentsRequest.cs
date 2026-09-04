using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests;

public class GetIssueCommentsRequest
{
    [Display("Issue key")]
    [DataSource(typeof(IssueDataSourceHandler))]
    public string IssueKey { get; set; }

    [Display("Issue comment IDs"), DataSource(typeof(IssueCommentDataHandler))]
    public IEnumerable<string>? Issues { get; set; }

    [Display("Limit", Description = "Maximum number of comments to return.")]
    public int? Limit { get; set; }

    [Display("Sort", Description = "Order comments by creation date.")]
    [StaticDataSource(typeof(CommentSortDataSource))]
    public string? Sort { get; set; }
}
