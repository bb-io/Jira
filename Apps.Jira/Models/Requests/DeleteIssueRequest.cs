using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests
{
    public class DeleteIssueRequest
    {
        [Display("Issue Key")]
        public string IssueKey { get; set; }
        
        [Display("Delete Subtasks")]
        public string DeleteSubtasks { get; set; }
    }
}