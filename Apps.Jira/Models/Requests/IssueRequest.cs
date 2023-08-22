﻿using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests
{
    public class IssueRequest
    {
        [Display("Issue")]
        [DataSource(typeof(IssueDataSourceHandler))]
        public string IssueKey { get; set; }
    }
}
