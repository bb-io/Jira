using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Jira.Webhooks.Handlers.IssueHandlers;

public class IssueCreatedOrUpdatedHandler : BaseWebhookHandler
{
    private static readonly string[] _subscriptionEvents = { "jira:issue_updated", "jira:issue_created" };
        
    public IssueCreatedOrUpdatedHandler(InvocationContext invocationContext) : base(invocationContext, _subscriptionEvents) { }
}