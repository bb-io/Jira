using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Webhooks.Inputs
{
    public class IssuesReachStatusInput
    {
        [Display("Issue keys", Description = "List of issue keys to watch, e.g. PROJ-1, PROJ-2")]
        [DataSource(typeof(IssueDataSourceHandler))]
        public IEnumerable<string> IssueKeys { get; set; }

        [Display("Statuses", Description = "Target statuses")]
        [DataSource(typeof(IssueStatusDataSourceHandler))]
        public IEnumerable<string> Statuses { get; set; }
    }
}
