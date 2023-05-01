using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Jira.Models.Responses
{
    public class IssueResponse
    {
        public string Summary { get; set; }

        public string Status { get; set; }

        public string Assignee { get; set; }

        public string Description { get; set; }
    }
}
