using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers;

public class IssueCommentIdentifier
{
    [Display("Issue key"), DataSource(typeof(IssueDataSourceHandler))]
    public string IssueKey { get; set; }
    
    [Display("Comment id"), DataSource(typeof(IssueCommentDataHandler))]
    public string CommentId { get; set; }
}