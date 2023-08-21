using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests
{
    public class IssueTransitionRequest
    {
        [Display("Transition ID")]
        public string TransitionId { get; set; }

        [Display("Issue key")]
        public string IssueKey { get; set; }
    }
}
