using Apps.Jira.DataSourceHandlers.CustomFields;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers;

public class CustomCascadingFieldIdentifier
{
    [Display("Custom cascading field ID")]
    [DataSource(typeof(CustomCascadingFieldDataSourceHandler))]
    public string CustomCascadingFieldId { get; set; } = default!;
}
