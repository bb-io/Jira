using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers;

public class CustomStringFieldIdentifier
{
    [Display("Custom string field")]
    [DataSource(typeof(CustomStringFieldDataSourceHandler))]
    public string CustomStringFieldId { get; set; }
}