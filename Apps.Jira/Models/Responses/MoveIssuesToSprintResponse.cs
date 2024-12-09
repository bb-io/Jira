

using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Responses
{
    public class MoveIssuesToSprintResponse
    {
        [Display("Status")]
        public bool Success { get; set; }

        [Display("Message")]
        public string? Message { get; set; }
    }
}
