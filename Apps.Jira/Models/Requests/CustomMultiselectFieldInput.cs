using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Jira.Models.Requests
{
    public class CustomMultiselectFieldInput
    {
        public IEnumerable<string> ValueProperty { get; set; }
    }
}
