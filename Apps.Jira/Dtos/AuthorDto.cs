using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos;

public class AuthorDto : Apps.Jira.Models.Responses.UserDto
{
    [Display("Time zone")]
    public string TimeZone { get; set; }
    
    [Display("Email address")]
    public string EmailAddress { get; set; }
}