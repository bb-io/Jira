using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos;

public class ProjectDto
{
    [Display("Project key")]
    public string Key { get; set; }
    
    [Display("Project name")]
    public string Name { get; set; }
}

public class ProjectWrapper
{
    public IEnumerable<ProjectDto> Values { get; set; }
}