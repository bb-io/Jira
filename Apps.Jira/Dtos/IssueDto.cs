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
            Status = issueWrapper.Fields.Status;
            Priority = issueWrapper.Fields.Priority;
            Assignee = issueWrapper.Fields.Assignee;
            Project = issueWrapper.Fields.Project;
            Description = issueWrapper.Fields.Description == null
                ? ""
                : string.Join('\n',
                    issueWrapper.Fields.Description.Content
                        .Select(x => string.Join('\n', x.Content.Select(c => c.Text)))
                        .ToArray());
        }
        
        [Display("Issue key")]
        public string IssueKey { get; set; }
        
        public string Summary { get; set; }
        
        public string? Description { get; set; }

        public StatusDto Status { get; set; }
        
        public PriorityDto Priority { get; set; }
        
        public ProjectDto Project { get; set; }

        public UserDto? Assignee { get; set; }
    }
}
