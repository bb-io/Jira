using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers;

public class ProjectIdentifier
{
    [Display("Project")]
    [DataSource(typeof(ProjectDataSourceHandler))]
    public string ProjectKey { get; set; }
}