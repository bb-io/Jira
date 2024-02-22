using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers;

public class CustomDateFieldIdentifier
{
    [Display("Custom date field ID")]
    [DataSource(typeof(CustomDateFieldDataSourceHandler))]
    public string CustomDateFieldId { get; set; }
}