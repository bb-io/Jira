﻿using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests
{
    public class IssueRequest
    {
        [Display("Issue Key")]
        public string IssueKey { get; set; }
    }
}
