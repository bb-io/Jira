namespace Apps.Jira.Dtos;

public record TransitionDto(string Id, string Name, StatusDto To);