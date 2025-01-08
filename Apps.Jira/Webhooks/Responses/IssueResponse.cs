using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Webhooks.Responses
{
    public class IssueResponse
    {
        [Display("Issue key")]
        public string IssueKey { get; set; }
        
        [Display("Project key")]
        public string ProjectKey { get; set; }
        
        public string Summary { get; set; }
        
        public string? Description { get; set; }
        
        [Display("Issue type")]
        public string IssueType { get; set; }
        
        public string? Priority { get; set; }
        
        [Display("Assignee account ID")]
        public string? AssigneeAccountId { get; set; }
        
        [Display("Assignee name")]
        public string? AssigneeName { get; set; }
      
        public string Status { get; set; }
        
        [Display("Due date")]
        public DateTime DueDate { get; set; }
        
        public IEnumerable<AttachmentDto> Attachments { get; set; }

        [Display("Labels")]
        public List<string> Labels { get; set; } = new();
    }
}