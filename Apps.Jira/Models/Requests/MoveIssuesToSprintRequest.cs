using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Jira.Models.Requests
{
    public class MoveIssuesToSprintRequest
    {
        public List<string> Issues { get; set; } = new();
        public string? RankAfterIssue { get; set; }
        public string? RankBeforeIssue { get; set; }
        public int RankCustomFieldId { get; set; }
    }
}
