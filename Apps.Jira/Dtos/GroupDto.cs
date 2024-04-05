using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos;

public class GroupDto
{
    public string Name { get; set; }
    
    [Display("Group ID")]
    public string GroupId { get; set; }
    
    public string Self { get; set; }
}