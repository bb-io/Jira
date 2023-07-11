using Apps.Jira.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Jira.Webhooks.Handlers.IssueHandlers
{
    public class IssueCreatedInProjectHandler : BaseWebhookHandler
    {
        private static readonly string[] _subscriptionEvents = { "jira:issue_created" };
        
        public IssueCreatedInProjectHandler([WebhookParameter] WebhookProjectInput input) : base(_subscriptionEvents, input) { }
       
        protected override string GetJqlFilter()
        {
            var webhookProjectInput = (WebhookProjectInput)webhookInput;
            string jqlFilter;
            if (webhookProjectInput == null || string.IsNullOrWhiteSpace(webhookProjectInput.ProjectKey))
                jqlFilter = "project != empty";
            else
                jqlFilter = $"project = {webhookProjectInput.ProjectKey}";
            return jqlFilter;
        }
    }
}