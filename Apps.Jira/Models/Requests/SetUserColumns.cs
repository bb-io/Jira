using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests;

public class SetUserColumns
{
    [Display("Columns", Description = "The columns to set for the user. For example: 'issuetype'")]
    public IEnumerable<string> Columns { get; set; }
}