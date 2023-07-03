namespace Apps.Jira.Models.Requests;

public class UpdateDescriptionRequest
{
    public string IssueKey { get; set; }
    public string Description { get; set; }
}