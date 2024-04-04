using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using Apps.Jira.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

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
    
    [Action("Get user", Description = "Get the specified user.")]
    public async Task<UserDto> GetUser([ActionParameter] UserIdentifier input)
    {
        var request = new JiraRequest($"/user?accountId={input.AccountId}", Method.Get);
        var user = await Client.ExecuteWithHandling<UserDto>(request);
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
}