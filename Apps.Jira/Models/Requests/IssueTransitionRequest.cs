namespace Apps.Jira.Models.Requests
{
    public class IssueTransitionRequest
    {
        public string TransitionId { get; set; }

        public string IssueKey { get; set; }
    }
}
