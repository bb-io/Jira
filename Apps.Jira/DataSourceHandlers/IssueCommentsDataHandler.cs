using Apps.Jira.Actions;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
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

        var comments = await issueCommentActions.GetIssueComments(new GetIssueCommentsRequest { IssueKey = _issueKey });
        return comments
            .Where(c => c?.Comment?.Id != null)
            .ToDictionary(
                c => c.Comment.Id,
                c => string.IsNullOrWhiteSpace(c.PlainText)
                    ? c.Comment.Id
                    : c.PlainText);
    }
}