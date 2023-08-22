using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests
{
    public class CreateIssueRequest
    {
        public string Summary { get; set; }

        [Display("Issue type")]
        [DataSource(typeof(IssueTypeDataSourceHandler))]
        public string IssueType { get; set; }
        
        public string? Description { get; set; }
    }
}