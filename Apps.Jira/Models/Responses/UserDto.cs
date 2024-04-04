namespace Apps.Jira.Models.Responses;

public class UserDto
{
    public string AccountId { get; set; }
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
    public string Size16 { get; set; }
    public string Size24 { get; set; }
    public string Size32 { get; set; }
    public string Size48 { get; set; }
}