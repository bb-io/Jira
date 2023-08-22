using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests
{
    public class AssignIssueRequest
    {
        [Display("Issue")]
        [DataSource(typeof(IssueDataSourceHandler))]
        public string IssueKey { get; set; }
        
        [Display("Assignee")]
        [DataSource(typeof(AssigneeDataSourceHandler))]
        public string? AccountId { get; set; }
    }
}