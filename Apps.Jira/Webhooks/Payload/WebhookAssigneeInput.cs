using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Webhooks.Payload
{
    public class WebhookAssigneeInput : IWebhookInput
    {
        [Display("Assignee Display Name or Id")]
        public string AssigneeNameOrId { get; set; }
    }
}