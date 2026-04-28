using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers.CustomFields;

public class CustomUserPickerFieldDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    private const string MultiUserPickerSchema =
        "com.atlassian.jira.plugin.system.customfieldtypes:multiuserpicker";

    public CustomUserPickerFieldDataSourceHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new JiraRequest("/field", Method.Get);
        var fields = await Client.ExecuteWithHandling<IEnumerable<FieldDto>>(request);
        var customUserPickerFields = fields
            .Where(field => field.Custom
                            && field.Schema?.Type == "array"
                            && field.Schema?.Items == "user"
                            && field.Schema?.Custom == MultiUserPickerSchema)
            .Where(field => context.SearchString == null ||
                            field.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));

        return customUserPickerFields.ToDictionary(field => field.Id, field => field.Name);
    }
}
