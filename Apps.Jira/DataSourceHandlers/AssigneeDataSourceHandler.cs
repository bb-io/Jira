using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers;

public class AssigneeDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    public AssigneeDataSourceHandler(InvocationContext invocationContext): base(invocationContext) { }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var baseEndpoint = "/user/search?maxResults=20&query=";

        if (!string.IsNullOrWhiteSpace(context.SearchString))
            baseEndpoint += context.SearchString;

        var accounts = new List<UserDto>();
        var startAt = 0;

        do
        {
            var endpoint = baseEndpoint + $"&startAt={startAt}";
            var request = new JiraRequest(endpoint, Method.Get);
            var response = await Client.ExecuteWithHandling<IEnumerable<UserDto>>(request);

            if (!response.Any())
                break;

            accounts.AddRange(response.Where(u => u.AccountType == "atlassian"));
            startAt += 20;
        } while (accounts.Count < 20);

        var accountsDictionary = accounts.ToDictionary(a => a.AccountId, a => a.DisplayName); 
        accountsDictionary.Add("-1", "Default assignee");
        accountsDictionary.Add(int.MinValue.ToString(), "Unassigned");
        return accountsDictionary;
    }
}