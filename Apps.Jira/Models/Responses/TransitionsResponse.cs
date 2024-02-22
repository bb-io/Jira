using Apps.Jira.Dtos;

namespace Apps.Jira.Models.Responses;

public record TransitionsResponse(IEnumerable<TransitionDto> Transitions);