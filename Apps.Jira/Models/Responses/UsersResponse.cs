using Apps.Jira.Dtos;

namespace Apps.Jira.Models.Responses;

public class UsersResponse
{
    public IEnumerable<UserDto> Users { get; set; }
}