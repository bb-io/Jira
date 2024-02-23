using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos
{
    public class UserDto
    {
        [Display("Account ID")]
        public string AccountId { get; set; }
        
        [Display("Email address")]
        public string? EmailAddress { get; set; }
        
        [Display("Display name")]
        public string DisplayName { get; set; }
        
        [Display("Account type")]
        public string? AccountType { get; set; }
    }
}
