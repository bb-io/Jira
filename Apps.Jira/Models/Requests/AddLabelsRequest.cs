namespace Apps.Jira.Models.Requests;

public class AddLabelsRequest
{
    public IEnumerable<string> Labels { get; set; } = new List<string>();
}