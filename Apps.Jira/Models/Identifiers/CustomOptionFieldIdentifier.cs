using Apps.Jira.DataSourceHandlers.CustomFields;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers;

public class CustomOptionFieldIdentifier
{
    [Display("Custom dropdown field ID")]
    [DataSource(typeof(CustomOptionFieldDataSourceHandler))]
    public string CustomOptionFieldId { get; set; }
}