using Apps.Jira.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Jira.Webhooks.Handlers.IssueHandlers
{
    public class IssueUpdatedHandler : BaseWebhookHandler
    {
        private static readonly string[] _subscriptionEvents = { "jira:issue_updated" };
        
        public IssueUpdatedHandler([WebhookParameter] WebhookIssueInput input) : base(_subscriptionEvents, input) { }
       
        protected override string GetJqlFilter()
        {
            var webhookIssueInput = (WebhookIssueInput)webhookInput;
            string jqlFilter;
            if (webhookIssueInput == null || string.IsNullOrWhiteSpace(webhookIssueInput.IssueKey))
                jqlFilter = "issueKey != empty";
            else
                jqlFilter = $"issueKey = {webhookIssueInput.IssueKey}";
            return jqlFilter;
        }
    }
}