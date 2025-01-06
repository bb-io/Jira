using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Webhooks.Inputs
{
    public class ProjectIssueInput
    {
        [Display("Projects")]
        [DataSource(typeof(ProjectDataSourceHandler))]
        public IEnumerable<string>? ProjectKey { get; set; }
    }
}
