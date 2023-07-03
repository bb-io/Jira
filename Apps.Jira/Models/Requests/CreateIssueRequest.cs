using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests
{
    public class CreateIssueRequest
    {
        [Display("Assignee Id")]
        public string? AssigneeId { get; set; }
        
        [Display("Project Key")]
        public string ProjectKey { get; set; }
        
        public string Summary { get; set; }
        
        public string? Description { get; set; }
        
        [Display("Issue Type")]
        public string IssueType { get; set; }
    }
}