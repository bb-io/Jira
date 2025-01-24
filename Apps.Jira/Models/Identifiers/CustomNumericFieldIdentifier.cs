using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.Jira.DataSourceHandlers.CustomFields;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers
{
    public class CustomNumericFieldIdentifier
    {
        [Display("Custom number field ID")]
        [DataSource(typeof(CustomNumericFieldDataSourceHandler))]
        public string CustomNumberFieldId { get; set; }
    }
}
