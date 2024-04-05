using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Responses;

public class UserDto
{
    [Display("Account ID")]
    public string AccountId { get; set; }
    
    [Display("Account ID")]
    public string AccountType { get; set; }
    
    public bool Active { get; set; }
    
    public AvatarUrls AvatarUrls { get; set; }
    public string DisplayName { get; set; }
    
    public string Key { get; set; }
    
    public string Name { get; set; }
    
    public string Self { get; set; }
}

public class AvatarUrls
{
    [Display("Size 16")]
    public string Size16 { get; set; }
    
    [Display("Size 24")]
    public string Size24 { get; set; }
    
    [Display("Size 32")]
    public string Size32 { get; set; }
    
    [Display("Size 48")]
    public string Size48 { get; set; }
}