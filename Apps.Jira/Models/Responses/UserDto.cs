using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Jira.Models.Responses;

public class UserDto
{
    [Display("Account ID")]
    public string AccountId { get; set; }
    
    [Display("Account type")]
    public string AccountType { get; set; }
    
    public bool Active { get; set; }
    
    [Display("Avatar URLs")]
    public AvatarUrls AvatarUrls { get; set; }
    
    [Display("Display name")]
    public string DisplayName { get; set; }
    
    public string Self { get; set; }

    [Display("E-mail address")]
    [JsonProperty("emailAddress")]
    public string EmailAddress { get; set; }
}

public class AvatarUrls
{
    [Display("Size 16"), JsonProperty("16x16")]
    public string Size16 { get; set; }
    
    [Display("Size 24"), JsonProperty("24x24")]
    public string Size24 { get; set; }
    
    [Display("Size 32"), JsonProperty("32x32")]
    public string Size32 { get; set; }
    
    [Display("Size 48"), JsonProperty("48x48")]
    public string Size48 { get; set; }
}