using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests;

public class LabelsOptionalInput
{
    [Display("Labels", Description = "Use this input if you want to filter results based on labels"), DataSource(typeof(LabelDataHandler))]
    public IEnumerable<string>? Labels { get; set; }
}