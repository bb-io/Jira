namespace Apps.Jira.Webhooks.Handlers.IssueHandlers
{
    public class IssueWithHighPriorityCreatedHandler : BaseWebhookHandler
    {
        private static readonly string[] _subscriptionEvents = { "jira:issue_created", "jira:issue_updated" };
        
        public IssueWithHighPriorityCreatedHandler() : base(_subscriptionEvents) { }
       
        protected override string GetJqlFilter()
        {
            string jqlFilter = "priority = High or priority = Highest";
            return jqlFilter;
        }
    }
}