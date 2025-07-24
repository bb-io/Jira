using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Responses;

public class IssuesResponse
{
    public IEnumerable<IssueDto> Issues { get; set; }

    [Display("Issues count")]
    public double Count { get; set; }
}