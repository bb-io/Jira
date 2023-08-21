using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Responses
{
    public class IssueResponse
    {
        [Display("Issue key")]
        public string IssueKey { get; set; }
        
        public string Summary { get; set; }

        public string Status { get; set; }
        
        public string Priority { get; set; }
        
        public string? Description { get; set; }
        
        public ProjectDto Project { get; set; }

        public UserDto? Assignee { get; set; }
    }
}
