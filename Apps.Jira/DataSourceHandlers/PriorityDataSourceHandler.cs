using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers;

public class PriorityDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    public PriorityDataSourceHandler(InvocationContext invocationContext): base(invocationContext) { }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new JiraRequest("/priority", Method.Get);
        var response = await Client.ExecuteWithHandling<IEnumerable<PriorityDto>>(request);
        return response.ToDictionary(p => p.Id, p => p.Name);
    }
}