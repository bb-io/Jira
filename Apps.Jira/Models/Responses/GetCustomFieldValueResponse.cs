namespace Apps.Jira.Models.Responses;

public class GetCustomFieldValueResponse<T>
{
    public T? Value { get; set; }
}