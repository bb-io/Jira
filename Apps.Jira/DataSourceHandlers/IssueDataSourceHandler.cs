using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers;

public class IssueDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    public IssueDataSourceHandler(InvocationContext invocationContext): base(invocationContext) { }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var endpoint = "/search?maxResults=20";

        if (!string.IsNullOrWhiteSpace(context.SearchString))
            endpoint += $"&jql=summary ~ '{context.SearchString}' OR description ~ '{context.SearchString}'";
        
        var request = new JiraRequest(endpoint, Method.Get);
        var response = await Client.ExecuteWithHandling<IssuesWrapper>(request);
        return response.Issues.ToDictionary(i => i.Key, i => $"{i.Fields.Summary} ({i.Fields.Project.Name} project)");
    }
}