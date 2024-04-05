using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos;

public class UserEmailDto
{
    [Display("Account ID")]
    public string AccountId { get; set; }
    
    public string Email { get; set; }
}