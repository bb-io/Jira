using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Webhooks.Inputs
{
    public class ParentIssueFilterInput
    {
        [Display("Parent issue key(s)")]
        [DataSource(typeof(IssueDataSourceHandler))]
        public IEnumerable<string>? ParentIssueKeys { get; set; }
    }
}
