namespace Apps.Jira.Models.Requests
{
    public class AssignIssueRequest
    {
        public string IssueKey { get; set; }
        public string? AccountId { get; set; }
    }
}