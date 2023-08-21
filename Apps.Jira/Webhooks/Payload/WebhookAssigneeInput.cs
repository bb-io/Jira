using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Webhooks.Payload
{
    public class WebhookAssigneeInput : IWebhookInput
    {
        [Display("Assignee display name or ID")]
        public string AssigneeNameOrId { get; set; }
    }
}