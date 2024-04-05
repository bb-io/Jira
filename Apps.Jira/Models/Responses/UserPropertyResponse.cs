namespace Apps.Jira.Models.Responses;

public class UserPropertyResponse<T>
{
    public string Key { get; set; }
    
    public T Value { get; set; }
}