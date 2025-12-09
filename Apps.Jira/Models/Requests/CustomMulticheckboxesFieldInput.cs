using Apps.Jira.DataSourceHandlers.CustomFields;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests
{
    public class CustomMulticheckboxesFieldInput
    {
        [Display("Values")]
        [DataSource(typeof(CustomMulticheckboxesOptionsDataSourceHandler))]
        public IEnumerable<string> Values { get; set; } = Enumerable.Empty<string>();
    }
}
