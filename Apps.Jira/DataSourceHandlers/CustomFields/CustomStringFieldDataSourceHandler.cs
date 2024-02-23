using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers.CustomFields;

public class CustomStringFieldDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    public CustomStringFieldDataSourceHandler(InvocationContext invocationContext): base(invocationContext) { }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new JiraRequest("/field", Method.Get);
        var fields = await Client.ExecuteWithHandling<IEnumerable<FieldDto>>(request);
        var customStringFields = fields
            .Where(field => field.Custom && (field.Schema!.Type == "string" || field.Schema.Type == "option"))
            .Where(field => context.SearchString == null ||
                            field.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));
        
        return customStringFields.ToDictionary(field => field.Id, field => field.Name);
    }
}