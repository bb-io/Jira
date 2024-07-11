namespace Apps.Jira.Models.Requests;

public class AddIssueCommentRequest
{
    public string Text { get; set; }

    public string? ContentType { get; set; }
        
    public string? Type { get; set; }

    public string? BodyType { get; set; }
        
    public string? Version { get; set; }
        
}