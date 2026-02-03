using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers;

public class IssueResolutionDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    public IssueResolutionDataSourceHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var req = new JiraRequest("/resolution", Method.Get);
        var response = await Client.ExecuteWithHandling(req);

        var arr = JArray.Parse(response.Content ?? "[]");

        var result = new Dictionary<string, string>();
        foreach (var r in arr)
        {
            var id = r?["id"]?.ToString();
            var name = r?["name"]?.ToString();
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                continue;

            if (!string.IsNullOrWhiteSpace(context.SearchString) &&
                !name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                continue;

            result[id] = name;
        }

        return result;
    }
}