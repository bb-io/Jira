using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers;

public class ProjectDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    public ProjectDataSourceHandler(InvocationContext invocationContext): base(invocationContext) { }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new JiraClient(Creds);
        var endpoint = "/project/search?maxResults=20";

        if (!string.IsNullOrWhiteSpace(context.SearchString))
            endpoint += $"&query={context.SearchString}";
        
        var request = new JiraRequest(endpoint, Method.Get, Creds);
        var response = await client.ExecuteWithHandling<ProjectWrapper>(request);
        return response.Values.ToDictionary(p => p.Key, p => p.Name);
    }
}