using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests
{
    public class IssueTransitionRequest
    {
        [Display("Transition Id")]
        public string TransitionId { get; set; }

        [Display("Issue Key")]
        public string IssueKey { get; set; }
    }
}
