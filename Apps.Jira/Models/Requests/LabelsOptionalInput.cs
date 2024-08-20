using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests;

public class LabelsOptionalInput
{
    [Display("Labels (manual)", Description = "Manually enter labels to filter results without using a dropdown. Use this input if you want to filter results based on labels")]
    public IEnumerable<string>? Labels { get; set; }
    
    [Display("Labels (dropdown selection)", Description = "Select labels from a dropdown to filter results. Use this input if you want to filter results based on labels"), DataSource(typeof(LabelDataHandler))]
    public IEnumerable<string>? LabelsDropDown { get; set; }
}