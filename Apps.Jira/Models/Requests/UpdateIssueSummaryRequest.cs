namespace Apps.Jira.Models.Requests;

public class UpdateIssueSummaryRequest
{
    public string IssueKey { get; set; }
    public string Summary { get; set; }
}