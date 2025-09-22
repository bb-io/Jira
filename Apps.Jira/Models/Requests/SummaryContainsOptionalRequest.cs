using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests;

public class SummaryContainsOptionalRequest
{
    [Display("Summary contains")]
    public string? Summary { get; set; }
}
