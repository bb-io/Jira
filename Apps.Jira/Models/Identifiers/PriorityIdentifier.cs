using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers;

public class PriorityIdentifier
{
    [Display("Priority")]
    [DataSource(typeof(PriorityDataSourceHandler))]
    public string PriorityId { get; set; }
}