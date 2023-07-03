namespace Apps.Jira.Models.Requests
{
    public class DeleteIssueRequest
    {
        public string IssueKey { get; set; }
        public string DeleteSubtasks { get; set; }
    }
}