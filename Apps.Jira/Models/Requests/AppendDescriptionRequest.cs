using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Jira.Models.Requests;

public class AppendDescriptionRequest
{
    [Display("Text", Description = "Text to append to the description")]
    public string Text { get; set; }

    [Display("Formatting", Description = "Formatting of the text"), StaticDataSource(typeof(FormattingDataSource))]
    public string? Formatting { get; set; }
}