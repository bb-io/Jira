using Apps.Jira.DataSourceHandlers.CustomFields;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers
{
    public class CustomMulticheckboxesFieldIdentifier
    {
        [Display("Custom field ID")]
        [DataSource(typeof(CustomMulticheckboxFieldDataSourceHandler))]
        public string CustomMulticheckboxesFieldId { get; set; } = default!;
    }
}
