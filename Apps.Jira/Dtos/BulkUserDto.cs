namespace Apps.Jira.Dtos;

public class BulkUserDto
{
    public string Self { get; set; }
    
    public int MaxResults { get; set; }
    
    public int StartAt { get; set; }
    
    public int Total { get; set; }
    
    public bool IsLast { get; set; }
    
    public UserBulk[] Values { get; set; }
}

public class UserBulk : Apps.Jira.Models.Responses.UserDto
{
    public string EmailAddress { get; set; }

    public bool Active { get; set; }

    public string TimeZone { get; set; }
}