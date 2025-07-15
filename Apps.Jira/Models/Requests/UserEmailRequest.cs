using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests
{
    public class UserEmailRequest
    {
        [Display("User email")]
        public string Email { get; set; }
    }
}
