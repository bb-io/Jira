using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Jira.Models.Responses
{
    public class MoveIssuesToSprintResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
