using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Responses
{
    public class GetIssueCommentsResponse
    {
        [Display("Comments")]
        public CommentWithTextResponse[] Comments { get; set; }
    }
}
