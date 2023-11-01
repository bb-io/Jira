using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Jira.Webhooks.Handlers.IssueHandlers;

public class IssueDeletedHandler : BaseWebhookHandler
{
    private static readonly string[] _subscriptionEvents = { "jira:issue_deleted" };
        
    public IssueDeletedHandler(InvocationContext invocationContext) : base(invocationContext, _subscriptionEvents) { }
}