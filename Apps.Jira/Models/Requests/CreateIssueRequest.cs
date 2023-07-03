namespace Apps.Jira.Models.Requests
{
    public class CreateIssueRequest
    {
        public string? AssigneeId { get; set; }
        public string ProjectKey { get; set; }
        public string Summary { get; set; }
        public string? Description { get; set; }
        public string IssueType { get; set; }
    }
}