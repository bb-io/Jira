using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Responses
{
    public class CommentWithTextResponse
    {
        [Display("Comment full details")]
        public IssueCommentDto Comment { get; set; }

        [Display("Plain comment text")]
        public string PlainText { get; set; }
    }
}
