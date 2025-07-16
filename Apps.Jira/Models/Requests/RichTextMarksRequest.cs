using Apps.Jira.DataSourceHandlers;
using Apps.Jira.DataSourceHandlers.Enum;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests;

public class RichTextMarksRequest
{
    [StaticDataSource(typeof(RichTextMarksHandler))]
    public IEnumerable<string>? Marks { get; set; }

    [Display("Link URL")]
    public string? LinkURL { get; set; }
}