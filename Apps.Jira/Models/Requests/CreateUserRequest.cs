namespace Apps.Jira.Models.Requests;

public class CreateUserRequest
{
    public string EmailAddress { get; set; }
    
    public IEnumerable<string>? Products { get; set; }
    
    public IEnumerable<string>? AdditionalPropertiesKeys { get; set; }

    public IEnumerable<string>? AdditionalPropertiesValues { get; set; }

    public CreateUserRequest()
    {
        Products = new List<string>();
        AdditionalPropertiesKeys = new List<string>();
        AdditionalPropertiesValues = new List<string>();
    }
}
