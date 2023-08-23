using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos
{
    public class IssueDto
    {
        public IssueDto() { }

        public IssueDto(IssueWrapper issueWrapper)
        {
            IssueKey = issueWrapper.Key;
            Summary = issueWrapper.Fields.Summary;
            Status = issueWrapper.Fields.Status.Name;
            Priority = issueWrapper.Fields.Priority.Name;
            Assignee = issueWrapper.Fields.Assignee;
            Project = issueWrapper.Fields.Project;
            Description = issueWrapper.Fields.Description == null
                ? ""
                : string.Join('\n',
                    issueWrapper.Fields.Description.Content
                        .Select(x => string.Join('\n', x.Content.Select(c => c.Text)))
                        .ToArray());
        }
        
        [Display("Issue")]
        public string IssueKey { get; set; }
        
        public string Summary { get; set; }

        public string Status { get; set; }
        
        public string Priority { get; set; }
        
        public string? Description { get; set; }
        
        public ProjectDto Project { get; set; }

        public UserDto? Assignee { get; set; }
    }
}
