using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos
{
    public class UserDto
    {
        [Display("Display name")]
        public string DisplayName { get; set; }

        [Display("Account ID")]
        public string AccountId { get; set; }
    }

    public class UsersResponseWrapper
    {
        public IEnumerable<UserDto> Users { get; set; }
    }
}
