using Apps.Jira.Actions;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Jira.DataSourceHandlers;

public class IssueCommentsDataHandler : JiraInvocable, IAsyncDataSourceHandler
{
    private readonly string _issueKey;

    public IssueCommentsDataHandler(InvocationContext invocationContext,
        [ActionParameter] GetIssueCommentsRequest identifier) : base(invocationContext)
    {
        _issueKey = identifier.IssueKey;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var issueCommentActions = new IssueCommentActions(InvocationContext);

        var response = await issueCommentActions.GetIssueComments(
            new GetIssueCommentsRequest { IssueKey = _issueKey });

        var comments = response.Comments ?? Array.Empty<CommentWithTextResponse>();

        return comments
            .Where(c => c?.Comment?.Id != null)
            .ToDictionary(
                c => c.Comment.Id,
                c =>
                {
                    var text = string.IsNullOrWhiteSpace(c.PlainText)
                        ? c.Comment.Id
                        : c.PlainText;

                    const int maxLen = 80;
                    return text.Length <= maxLen
                        ? text
                        : text.Substring(0, maxLen) + "…";
                });
    }
}