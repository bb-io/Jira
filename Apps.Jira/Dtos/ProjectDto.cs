using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Jira.Dtos;

public class ProjectDto
{
    [Display("Project key")]
    public string Key { get; set; }
    
    [Display("Project ID")]
    public string Id { get; set; }
    
    [Display("Project name")]
    public string Name { get; set; }
}

public class DetailedProjectDto : ProjectDto
{
    public string Description { get; set; }
    
    public UserDto Lead { get; set; }
    
    [Display("Issue types")]
    public IEnumerable<IssueTypeDto> IssueTypes { get; set; }
    
    [JsonProperty("assigneeType")]
    [Display("Default assignee")]
    public string DefaultAssignee { get; set; }
    
    [Display("Project type key")]
    public string ProjectTypeKey { get; set; }
}

public class ProjectWrapper
{
    public IEnumerable<ProjectDto> Values { get; set; }
}