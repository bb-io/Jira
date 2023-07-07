using Apps.Jira.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Jira.Webhooks.Handlers.IssueHandlers
{
    public class IssueAssignedHandler : BaseWebhookHandler
    {
        private static readonly string[] _subscriptionEvents = { "jira:issue_updated", "jira:issue_created" };
        
        public IssueAssignedHandler([WebhookParameter] WebhookAssigneeInput input) : base(_subscriptionEvents, input) { }
       
        protected override string GetJqlFilter()
        {
            var webhookAssigneeInput = (WebhookAssigneeInput)webhookInput;
            string jqlFilter = $"assignee = \"{webhookAssigneeInput.AssigneeNameOrId}\"";
            return jqlFilter;
        }
    }
}