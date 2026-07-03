using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests;

public class AddIssueCommentRequest
{
    public string? Text { get; set; }

    [Display("Link URL")]
    public string? LinkUrl { get; set; }

    [Display("Link text")]
    public string? LinkText { get; set; }

    [Display("Mention users")]
    [DataSource(typeof(UserDataSourceHandler))]
    public IEnumerable<string>? MentionAccountIds { get; set; }
}
