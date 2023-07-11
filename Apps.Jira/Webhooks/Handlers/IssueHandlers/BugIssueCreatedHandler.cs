namespace Apps.Jira.Webhooks.Handlers.IssueHandlers
{
    public class BugIssueCreatedHandler : BaseWebhookHandler
    {
        private static readonly string[] _subscriptionEvents = { "jira:issue_updated", "jira:issue_created" };
        
        public BugIssueCreatedHandler() : base(_subscriptionEvents) { }
       
        protected override string GetJqlFilter()
        {
            string jqlFilter = "issuetype = Bug";
            return jqlFilter;
        }
    }
}