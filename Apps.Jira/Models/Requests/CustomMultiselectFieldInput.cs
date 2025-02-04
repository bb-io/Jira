using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests
{
    public class CustomMultiselectFieldInput
    {
        [Display("Multiselect value field")]
        public IEnumerable<string> ValueProperty { get; set; }
    }
}
