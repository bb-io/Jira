using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests
{
    public class CreateIssueRequest
    {
        public string Summary { get; set; }

        [Display("Issue type ID")]
        [DataSource(typeof(IssueTypeDataSourceHandler))]
        public string IssueTypeId { get; set; }
        
        public string? Description { get; set; }
        
        [Display("Assignee account ID")]
        [DataSource(typeof(AssigneeDataSourceHandler))]
        public string? AccountId { get; set; }
    }
}