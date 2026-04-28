using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests;

public class CustomUserPickerFieldInput
{
    [Display("Account IDs")]
    [DataSource(typeof(AssigneeDataSourceHandler))]
    public IEnumerable<string> AccountIds { get; set; } = Enumerable.Empty<string>();
}
