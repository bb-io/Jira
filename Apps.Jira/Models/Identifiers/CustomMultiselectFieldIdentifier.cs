using Apps.Jira.DataSourceHandlers.CustomFields;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers;

public class CustomMultiselectFieldIdentifier
{
    [Display("Custom multiselect field ID")]
    [DataSource(typeof(CustomMultiselectFieldDataSourceHandler))]
    public string CustomMultiselectFieldId { get; set; }
}