using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Webhooks.Responses
{
    public class IssueResponse
    {
        [Display("Issue Key")]
        public string IssueKey { get; set; }
        
        [Display("Project Key")]
        public string ProjectKey { get; set; }
        
        public string Summary { get; set; }
        
        public string Description { get; set; }
        
        [Display("Issue Type")]
        public string IssueType { get; set; }
        
        public string Priority { get; set; }
        
        [Display("Assignee Account Id")]
        public string AssigneeAccountId { get; set; }
        
        [Display("Assignee Name")]
        public string AssigneeName { get; set; }
        
        public string Status { get; set; }
    }
}