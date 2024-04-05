namespace Apps.Jira.Models.Responses;

public class UserPropertiesResponse
{
    public List<KeyDto> Keys { get; set; }
}

public class KeyDto
{
    public string Key { get; set; }
    public string Self { get; set; }
}