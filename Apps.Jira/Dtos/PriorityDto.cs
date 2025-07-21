using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos;

public class PriorityDto
{
    [Display("Priority ID")]
    public string Id { get; set; }
    public string Name { get; set; }
}