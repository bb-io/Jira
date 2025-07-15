using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using Apps.Jira.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;
using UserDto = Apps.Jira.Models.Responses.UserDto;

namespace Apps.Jira.Actions;

[ActionList]
public class UserActions : JiraInvocable
{
    public UserActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }
    
    [Action("List users", Description = "List users.")]
    public async Task<UsersResponse> ListUsers()
    {
        var request = new JiraRequest("/users/search?maxResults=20", Method.Get);
        var users = await Client.ExecuteWithHandling<List<UserDto>>(request);
        return new UsersResponse { Users = users };
    }

    [Action("Find user by email", Description = "Finds user by email")]
    public async Task<UsersResponse> FindUserByEmail([ActionParameter] UserEmailRequest input)
    {
        var request = new JiraRequest($"/users/search?query={input.Email}", Method.Get);
        var users = await Client.ExecuteWithHandling<List<UserDto>>(request) ?? new List<UserDto>();

        var filtered = users
        .Where(u => u.AccountType.Equals("atlassian", StringComparison.OrdinalIgnoreCase))
        .Where(u => string.Equals(u.EmailAddress, input.Email, StringComparison.OrdinalIgnoreCase))
        .ToList();

        return new UsersResponse { Users = filtered };
    }

    [Action("Get user", Description = "Get the specified user.")]
    public async Task<ExpandedUserDto> GetUser([ActionParameter] UserIdentifier input)
    {
        var request = new JiraRequest($"/user?accountId={input.AccountId}", Method.Get);
        var user = await Client.ExecuteWithHandling<ExpandedUserDto>(request);
        return user;
    }
    
    [Action("Delete user", Description = "Delete the specified user.")]
    public async Task DeleteUser([ActionParameter] UserIdentifier input)
    {
        var request = new JiraRequest($"/user?accountId={input.AccountId}", Method.Delete);
        await Client.ExecuteWithHandling(request);
    }
    
    [Action("Create user", Description = "Create a user.")]
    public async Task<UserDto> CreateUser([ActionParameter] CreateUserRequest input)
    {
        var request = new JiraRequest("/user", Method.Post)
            .AddJsonBody(new
            {
                emailAddress = input.EmailAddress,
                productKeys = input.Products,
                additionalProperties = EnumerableUtils.ToDictionary(input.AdditionalPropertiesKeys, input.AdditionalPropertiesValues)
            });
        
        var user = await Client.ExecuteWithHandling<UserDto>(request);
        return user;
    }
    
    [Action("Get groups", Description = "Get the groups for the specified user.")]
    public async Task<List<GroupDto>> GetGroups([ActionParameter] UserIdentifier input)
    {
        var request = new JiraRequest($"/user/groups?accountId={input.AccountId}", Method.Get);
        var groups = await Client.ExecuteWithHandling<List<GroupDto>>(request);
        return groups;
    }
    
    [Action("Get user email", Description = "Get the email for the specified user.")]
    public async Task<UserEmailDto> GetUserEmail([ActionParameter] UserIdentifier input)
    {
        var request = new JiraRequest($"/user/email?accountId={input.AccountId}", Method.Get);
        var email = await Client.ExecuteWithHandling<UserEmailDto>(request);
        return email;
    }
    
    [Action("Get user columns", Description = "Get the columns for the specified user.")]
    public async Task<List<ColumnDto>> GetUserColumns([ActionParameter] UserIdentifier input)
    {
        var request = new JiraRequest($"/user/columns?accountId={input.AccountId}", Method.Get);
        var columns = await Client.ExecuteWithHandling<List<ColumnDto>>(request);
        return columns;
    }
    
    [Action("Reset user default columns", Description = "Reset the default columns for the specified user.")]
    public async Task ResetUserDefaultColumns([ActionParameter] UserIdentifier input)
    {
        var request = new JiraRequest($"/user/columns?accountId={input.AccountId}", Method.Delete);
        await Client.ExecuteWithHandling(request);
    }
    
    [Action("Set user columns", Description = "Set the columns for the specified user.")]
    public async Task SetUserColumns([ActionParameter] UserIdentifier input, [ActionParameter] SetUserColumns columnsRequest)
    {
        var request = new JiraRequest($"/user/columns?accountId={input.AccountId}", Method.Put)
            .AddJsonBody(new
            {
                columns = columnsRequest.Columns.ToList()
            });
        
        await Client.ExecuteWithHandling(request);
    }
    
    [Action("Bulk get users", Description = "Bulk get users.")]
    public async Task<List<BulkUserDto>> BulkGetUsers([ActionParameter] IEnumerable<UserIdentifier> inputs)
    {
        var request = new JiraRequest("/users/bulk", Method.Post)
            .AddJsonBody(inputs.Select(x => new { accountId = x.AccountId }));
        
        var users = await Client.ExecuteWithHandling<List<BulkUserDto>>(request);
        return users;
    }
}