using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Jira.DataSourceHandlers.Enum
{
    public class FieldDataHandler : IStaticDataSourceHandler
    {
        protected Dictionary<string, string> EnumValues => new()
        {
            {"issuetype", "Issue Type"},
            {"duedate", "Due Date"},
            {"summary","Summary" },
            {"watches", "Watchers" },
            {"statuscategorychangedate", "Status Category Changed" }
        };
        public Dictionary<string, string> GetData()
        {
            return EnumValues;
        }
    }
}
