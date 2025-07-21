using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos;

public class StatusDto
{
    [Display("Status ID")]
    public string Id { get; set; }
    public string Name { get; set; }
}

public record StatusesWrapper(IEnumerable<StatusDto> Statuses);