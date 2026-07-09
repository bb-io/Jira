using Apps.Jira.DataSourceHandlers.CustomFields;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests;

public class CustomCascadingFieldValueInput
{
    [Display("Parent option ID")]
    [DataSource(typeof(CustomCascadingParentOptionsDataSourceHandler))]
    public string ParentOptionId { get; set; } = default!;

    [Display("Child option ID")]
    [DataSource(typeof(CustomCascadingChildOptionsDataSourceHandler))]
    public string? ChildOptionId { get; set; }
}
