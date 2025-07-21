using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests;

public class ListRecentlyCreatedIssuesRequest
{
    [Display("Created hours ago (24 hours by default)")]
    public int? Hours { get; set; }

    [Display("Labels")]
    [DataSource(typeof(LabelDataHandler))]
    public IEnumerable<string>? Labels { get; set; }

    [Display("Fix versions")]
    [DataSource(typeof(FixVersionDataHandler))]
    public IEnumerable<string>? Versions { get; set; }
}
