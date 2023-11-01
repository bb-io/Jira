using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Jira.Webhooks.Handlers.IssueHandlers
{
    public class IssueUpdatedHandler : BaseWebhookHandler
    {
        private static readonly string[] _subscriptionEvents = { "jira:issue_updated" };
        
        public IssueUpdatedHandler(InvocationContext invocationContext) : base(invocationContext, _subscriptionEvents) { }
    }
}