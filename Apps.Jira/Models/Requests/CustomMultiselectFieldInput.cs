using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests
{
    public class CustomMultiselectFieldInput
    {
        [Display("Multiselect value field")]
        public IEnumerable<string> ValueProperty { get; set; }
    }
}
