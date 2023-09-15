namespace Apps.Jira.Webhooks.Handlers.IssueHandlers;

public class IssueDeletedHandler : BaseWebhookHandler
{
    private static readonly string[] _subscriptionEvents = { "jira:issue_deleted" };
        
    public IssueDeletedHandler() : base(_subscriptionEvents) { }
}