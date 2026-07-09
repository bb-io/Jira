using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Responses;

public class GetCustomCascadingFieldValueResponse
{
    [Display("Parent option ID")]
    public string? ParentOptionId { get; set; }

    [Display("Parent value")]
    public string? ParentValue { get; set; }

    [Display("Child option ID")]
    public string? ChildOptionId { get; set; }

    [Display("Child value")]
    public string? ChildValue { get; set; }
}
