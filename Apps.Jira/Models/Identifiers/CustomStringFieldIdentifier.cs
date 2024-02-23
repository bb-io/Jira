using Apps.Jira.DataSourceHandlers.CustomFields;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers;

public class CustomStringFieldIdentifier
{
    [Display("Custom string field ID")]
    [DataSource(typeof(CustomStringFieldDataSourceHandler))]
    public string CustomStringFieldId { get; set; }
}