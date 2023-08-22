using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests
{
    public class CreateIssueRequest
    {
        [Display("Assignee")]
        [DataSource(typeof(AssigneeDataSourceHandler))]
        public string? AssigneeId { get; set; }
        
        [Display("Project")]
        [DataSource(typeof(ProjectDataSourceHandler))]
        public string ProjectKey { get; set; }
        
        public string Summary { get; set; }
        
        public string? Description { get; set; }
        
        [Display("Issue type")]
        [DataSource(typeof(IssueTypeDataSourceHandler))]
        public string IssueType { get; set; }
    }
}