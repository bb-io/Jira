using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests;

public class AddIssueCommentRequest
{
    public string Text { get; set; }

    [Display("Content type")]
    public string? ContentType { get; set; }
        
    public string? Type { get; set; }

    [Display("Body type")]
    public string? BodyType { get; set; }
        
    public string? Version { get; set; }

    [Display("Mention users")]
    [DataSource(typeof(UserDataSourceHandler))]
    public IEnumerable<string>? MentionAccountIds { get; set; }
}