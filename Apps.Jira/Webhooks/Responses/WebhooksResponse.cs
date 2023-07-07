namespace Apps.Jira.Webhooks.Responses
{
    public class WebhooksResponse
    {
        public int MaxResults { get; set; }
        public int StartAt { get; set; }
        public int Total { get; set; }
        public bool IsLast { get; set; }
        public List<WebhookDto> Values { get; set; }
    }

    public class WebhookDto
    {
        public string Id { get; set; }
        public List<string> Events { get; set; }
        public string JqlFilter { get; set; }
        public string ExpirationDate { get; set; }
    }
}