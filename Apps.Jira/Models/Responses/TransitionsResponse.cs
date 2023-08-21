using Apps.Jira.Dtos;

namespace Apps.Jira.Models.Responses;

public class TransitionsResponse
{
    public IEnumerable<TransitionDto> Transitions { get; set; }
}