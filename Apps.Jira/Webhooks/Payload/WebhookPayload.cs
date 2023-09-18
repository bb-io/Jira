namespace Apps.Jira.Webhooks.Payload;

public class WebhookPayload
{
    public string WebhookEvent { get; set; }
    public Issue Issue { get; set; }
    public Changelog Changelog { get; set; }
}