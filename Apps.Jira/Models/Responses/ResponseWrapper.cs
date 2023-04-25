using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Jira.Models.Responses
{
    public class ResponseWrapper<T>
    {
        public T Fields { get; set; }
    }
}
