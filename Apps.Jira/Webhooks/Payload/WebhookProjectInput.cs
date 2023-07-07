using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Webhooks.Payload
{
    public class WebhookProjectInput : IWebhookInput
    {
        [Display("Project Key")]
        public string? ProjectKey { get; set; }
    }
}