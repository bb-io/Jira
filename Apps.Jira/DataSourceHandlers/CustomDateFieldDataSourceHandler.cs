using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers;

public class CustomDateFieldDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    public CustomDateFieldDataSourceHandler(InvocationContext invocationContext): base(invocationContext) { }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new JiraClient(Creds);
        var request = new JiraRequest("/field", Method.Get, Creds);
        var fields = await client.ExecuteWithHandling<IEnumerable<FieldDto>>(request);
        var customDateFields = fields
            .Where(field => field.Custom && (field.Schema!.Type == "date" || field.Schema.Type == "datetime"))
            .Where(field => context.SearchString == null ||
                            field.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));
        
        return customDateFields.ToDictionary(field => field.Id, field => field.Name);
    }
}