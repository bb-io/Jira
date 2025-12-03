using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Responses
{
    public class CommentResponse
    {
        [Display("Comment full details")]
        public IssueCommentDto CommentDto { get; set; }

        [Display("Plain comment text")]
        public string CommentFlattenedText { get; set; }
    }
}
