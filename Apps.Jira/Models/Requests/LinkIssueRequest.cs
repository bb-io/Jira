using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests
{
    public class LinkIssueRequest
    {
        [Display("Inward issue key", Description = "The issue that is the source of the link")]
        [DataSource(typeof(IssueDataSourceHandler))]
        public string InwardIssueKey { get; set; }

        [Display("Outward issue key", Description = "The issue that is the target of the link")]
        [DataSource(typeof(IssueDataSourceHandler))]
        public string OutwardIssueKey { get; set; }

        [Display("Link type", Description = "The type of link between the issues")]
        [DataSource(typeof(IssueLinkTypeDataSourceHandler))]
        public string LinkTypeName { get; set; }

        [Display("Comment", Description = "Optional comment to add to the link")]
        public string? Comment { get; set; }
    }
}
