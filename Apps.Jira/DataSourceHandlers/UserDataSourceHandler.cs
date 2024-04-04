using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers;

public class UserDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    public UserDataSourceHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var endpoint = "/users/search?maxResults=20";

        if (!string.IsNullOrWhiteSpace(context.SearchString))
            endpoint += $"&query={context.SearchString}";

        var request = new JiraRequest(endpoint, Method.Get);
        var response = await Client.ExecuteWithHandling<List<UserDto>>(request);
        return response.ToDictionary(p => p.AccountId, p => p.DisplayName);
    }
}