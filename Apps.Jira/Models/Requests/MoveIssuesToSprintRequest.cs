using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.Jira.DataSourceHandlers;
using Apps.MemoQ.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests
{
    public class MoveIssuesToSprintRequest
    {
        [Display("Board ID")]
        [DataSource(typeof(BoardDataSourceHandler))]
        public string BoardId { get; set; }

        [Display("Board ID")]
        [DataSource(typeof(SprintDataHandler))]
        public string SprintId { get; set; }

        [Display("Issues")]
        [DataSource(typeof(IssueDataSourceHandler))]
        public IEnumerable<string> Issues { get; set; }

        [Display("Rank after issue")]
        public string? RankAfterIssue { get; set; }

        [Display("Rank before issue")]
        public string? RankBeforeIssue { get; set; }

        [Display("Rank custom field")]
        public int? RankCustomFieldId { get; set; }
    }
}
