namespace Apps.Jira.Dtos;

public record StatusDto(string Id, string Name);

public record StatusesWrapper(IEnumerable<StatusDto> Statuses);