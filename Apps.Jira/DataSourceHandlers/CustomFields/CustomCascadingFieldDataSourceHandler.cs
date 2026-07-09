using Apps.Jira.Contants;
using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers.CustomFields;

public class CustomCascadingFieldDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    public CustomCascadingFieldDataSourceHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new JiraRequest("/field", Method.Get);
        var fields = await Client.ExecuteWithHandling<IEnumerable<FieldDto>>(request);
        var cascadingFields = fields
            .Where(field => field.Custom
                            && field.Schema?.Type == "option"
                            && string.Equals(field.Schema?.Custom, CustomFieldTypeIds.CascadingSelect,
                                StringComparison.OrdinalIgnoreCase))
            .Where(field => context.SearchString == null ||
                            field.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));

        return cascadingFields.ToDictionary(field => field.Id, field => field.Name);
    }
}
