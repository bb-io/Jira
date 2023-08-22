using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers;

public class IssueTypeDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    public IssueTypeDataSourceHandler(InvocationContext invocationContext): base(invocationContext) { }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new JiraClient(Creds);
        var endpoint = "/issuetype";
        var request = new JiraRequest(endpoint, Method.Get, Creds);
        var response = await client.ExecuteWithHandling<IEnumerable<IssueTypeDto>>(request);
        var issueTypes = response
            .Select(i => i.Name)
            .Distinct()
            .Where(i => context.SearchString == null || i.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(i => i, i => i);
        return issueTypes;
    }
}