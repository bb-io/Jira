using System.Reflection;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.Actions;

[ActionList]
public class UserPropertiesAction : JiraInvocable
{
    public UserPropertiesAction(InvocationContext invocationContext) : base(invocationContext)
    {
    }
    
    [Action("Get user properties", Description = "Get the properties for the specified user.")]
    public async Task<UserPropertiesResponse> GetUserProperties([ActionParameter] UserIdentifier input)
    {
        var request = new JiraRequest($"/user/properties?accountId={input.AccountId}", Method.Get);
        var properties = await Client.ExecuteWithHandling<UserPropertiesResponse>(request);
        return properties;
    }
    
    [Action("Get boolean user property", Description = "Get the specified property for the specified user.")]
    public async Task<UserPropertyResponse<bool>> GetBooleanUserProperty([ActionParameter] GetUserPropertyRequest input)
    {
        var request = new JiraRequest($"/user/properties/{input.PropertyKey}?accountId={input.AccountId}", Method.Get);
        var property = await Client.ExecuteWithHandling<UserPropertyResponse<bool>>(request);
        return property;
    }
    
    [Action("Get string user property", Description = "Get the specified string property for the specified user.")]
    public async Task<UserPropertyResponse<string>> GetStringUserProperty([ActionParameter] GetUserPropertyRequest input)
    {
        var request = new JiraRequest($"/user/properties/{input.PropertyKey}?accountId={input.AccountId}", Method.Get);
        var property = await Client.ExecuteWithHandling<UserPropertyResponse<string>>(request);
        return property;
    }

    [Action("Get integer user property", Description = "Get the specified integer property for the specified user.")]
    public async Task<UserPropertyResponse<int>> GetIntegerUserProperty([ActionParameter] GetUserPropertyRequest input)
    {
        var request = new JiraRequest($"/user/properties/{input.PropertyKey}?accountId={input.AccountId}", Method.Get);
        var property = await Client.ExecuteWithHandling<UserPropertyResponse<int>>(request);
        return property;
    }

    [Action("Get date user property", Description = "Get the specified date property for the specified user.")]
    public async Task<UserPropertyResponse<DateTime>> GetDateUserProperty([ActionParameter] GetUserPropertyRequest input)
    {
        var request = new JiraRequest($"/user/properties/{input.PropertyKey}?accountId={input.AccountId}", Method.Get);
        var property = await Client.ExecuteWithHandling<UserPropertyResponse<DateTime>>(request);
        return property;
    }

    [Action("Get array user property", Description = "Get the specified array property for the specified user.")]
    public async Task<UserPropertyResponse<string[]>> GetArrayUserProperty([ActionParameter] GetUserPropertyRequest input)
    {
        var request = new JiraRequest($"/user/properties/{input.PropertyKey}?accountId={input.AccountId}", Method.Get);
        var property = await Client.ExecuteWithHandling<UserPropertyResponse<string[]>>(request);
        return property;
    }
    
    [Action("Set user property", Description = "Set the specified property for the specified user.")]
    public async Task SetUserProperty([ActionParameter] GetUserPropertyRequest input, 
        [ActionParameter] SetUserPropertyRequest userProperty)
    {
        var properties = userProperty.GetType().GetProperties();
        var nonNullProperties = properties
            .Where(prop => prop.GetValue(userProperty) != null)
            .ToList();

        if (nonNullProperties.Count == 0)
        {
            throw new ArgumentException("No property was set. Please specify one property to set.");
        }
        
        if (nonNullProperties.Count > 1)
        {
            throw new ArgumentException("More than one property was set. Please specify only one property at a time.");
        }

        PropertyInfo property = nonNullProperties.First();
        object value = property.GetValue(userProperty)!;

        var request = new JiraRequest($"/user/properties/{input.PropertyKey}?accountId={input.AccountId}", Method.Put);
        request.AddJsonBody(value);
        
        await Client.ExecuteWithHandling(request);
    }
    
    [Action("Delete user property", Description = "Delete the specified property for the specified user.")]
    public async Task DeleteUserProperty([ActionParameter] GetUserPropertyRequest input)
    {
        var request = new JiraRequest($"/user/properties/{input.PropertyKey}?accountId={input.AccountId}", Method.Delete);
        await Client.ExecuteWithHandling(request);
    }
}