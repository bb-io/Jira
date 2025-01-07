using Apps.Jira.DataSourceHandlers;
using Apps.Jira.DataSourceHandlers.Enum;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Webhooks.Inputs;

public class ProjectInput
{
    [Display("Projects")]
    [DataSource(typeof(ProjectDataSourceHandler))]
    public IEnumerable<string>? ProjectKey { get; set; }

    [Display("Fields")]
    [StaticDataSource(typeof(FieldDataHandler))]
    public IEnumerable<string>? Field { get; set; }
}