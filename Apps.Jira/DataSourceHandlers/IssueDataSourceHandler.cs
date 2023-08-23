using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers;

public class IssueDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    public IssueDataSourceHandler(InvocationContext invocationContext): base(invocationContext) { }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new JiraClient(Creds);
        var endpoint = "/search?maxResults=20";

        if (!string.IsNullOrWhiteSpace(context.SearchString))
            endpoint += $"&jql=summary ~ '{context.SearchString}' OR description ~ '{context.SearchString}'";
        
        var request = new JiraRequest(endpoint, Method.Get, Creds);
        var response = await client.ExecuteWithHandling<IssuesWrapper>(request);
        return response.Issues.ToDictionary(i => i.Key, i => $"{i.Fields.Summary} ({i.Fields.Project.Name} project)");
    }
}