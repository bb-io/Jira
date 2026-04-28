using Apps.Jira.DataSourceHandlers.CustomFields;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers;

public class CustomUserPickerFieldIdentifier
{
    [Display("Custom user picker field ID")]
    [DataSource(typeof(CustomUserPickerFieldDataSourceHandler))]
    public string CustomUserPickerFieldId { get; set; } = default!;
}
