using Apps.Jira.Dtos;

namespace Apps.Jira.Models.Responses;

public class IssuesResponse
{
    public IEnumerable<IssueDto> Issues { get; set; }
}