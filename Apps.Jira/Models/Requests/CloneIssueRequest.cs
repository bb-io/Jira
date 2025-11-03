using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests
{
    public class CloneIssueRequest
    {
        [Display("Copy status")]
        public bool? CopyStatus { get; set; }

        [Display("New summary")]
        public string? NewSummary { get; set; }

        [Display("New description")]
        public string? NewDescription { get; set; }

        [Display("Link type", Description = "The type of link between the issues")]
        [DataSource(typeof(IssueLinkTypeDataSourceHandler))]
        public string LinkTypeName { get; set; }

        [Display("Comment", Description = "Optional comment to add to the link")]
        public string? Comment { get; set; }

        [Display("Assignee account name")]
        [DataSource(typeof(AssigneeDataSourceHandler))]
        public string? AssigneeName { get; set; }

        [Display("Reporter name")]
        [DataSource(typeof(AssigneeDataSourceHandler))]
        public string? ReporterName { get; set; }
    }

}
