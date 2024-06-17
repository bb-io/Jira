using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Jira.Webhooks.Inputs
{
    public class OptionalStatusInput
    {
        [Display("Status (transition) ID")]
        [DataSource(typeof(IssueStatusDataSourceHandler))]
        public string? StatusId { get; set; }
    }
}
