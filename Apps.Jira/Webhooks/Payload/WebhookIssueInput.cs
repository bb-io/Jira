using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Webhooks.Payload
{
    public class WebhookIssueInput : IWebhookInput
    {
        [Display("Issue Key")]
        public string? IssueKey { get; set; }
    }
}

