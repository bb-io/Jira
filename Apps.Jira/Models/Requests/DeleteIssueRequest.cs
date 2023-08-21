using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests
{
    public class DeleteIssueRequest
    {
        [Display("Issue key")]
        public string IssueKey { get; set; }
        
        [Display("Delete subtasks")]
        public string DeleteSubtasks { get; set; }
    }
}