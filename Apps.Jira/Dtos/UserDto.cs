namespace Apps.Jira.Dtos
{
    public class UserDto
    {
        public string DisplayName { get; set; }

        public string AccountId { get; set; }
    }

    public class UsersResponseWrapper
    {
        public IEnumerable<UserDto> Users { get; set; }
    }
}
