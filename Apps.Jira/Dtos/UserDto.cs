using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
