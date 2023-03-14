using Apps.Jira.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Jira.Models.Responses
{
    public class GetIssueTransitionsResponse
    {
        public IEnumerable<TransitionDto> Transitions { get; set; }
    }
}
