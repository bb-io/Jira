using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests
{
    public class CreateIssueRequest
    {
        [Display("Assignee ID")]
        public string? AssigneeId { get; set; }
        
        [Display("Project key")]
        public string ProjectKey { get; set; }
        
        public string Summary { get; set; }
        
        public string? Description { get; set; }
        
        [Display("Issue type")]
        public string IssueType { get; set; }
    }
}