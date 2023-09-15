namespace Apps.Jira.Webhooks.Handlers.IssueHandlers;

public class IssueCreatedHandler : BaseWebhookHandler
{
    private static readonly string[] _subscriptionEvents = { "jira:issue_created" };
        
    public IssueCreatedHandler() : base(_subscriptionEvents) { }
}