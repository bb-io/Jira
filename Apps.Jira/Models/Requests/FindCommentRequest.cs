using Apps.Jira.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests;

public class FindCommentRequest : IssueIdentifier
{
    [Display("Comment text contains")]
    public string CommentContains { get; set; }

    [Display("Latest")]
    public bool? Latest { get; set; }
}
