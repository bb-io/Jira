namespace Apps.Jira.Models.Requests;

public class PrioritizeIssueRequest
{
    public string IssueKey { get; set; }
    public string PriorityId { get; set; }
}