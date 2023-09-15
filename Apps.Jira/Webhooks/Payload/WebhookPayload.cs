namespace Apps.Jira.Webhooks.Payload;

public class WebhookPayload
{
    public Issue Issue { get; set; }
    public Changelog Changelog { get; set; }
}