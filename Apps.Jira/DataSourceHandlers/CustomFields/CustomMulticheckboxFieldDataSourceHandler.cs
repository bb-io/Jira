using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers.CustomFields
{
    public class CustomMulticheckboxFieldDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
    {
        public CustomMulticheckboxFieldDataSourceHandler(InvocationContext invocationContext) : base(invocationContext) { }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var request = new JiraRequest("/field", Method.Get);
            var fields = await Client.ExecuteWithHandling<IEnumerable<FieldDto>>(request);
            var customCheckboxFields = fields
                .Where(field => field.Custom
                                && field.Schema!.Type == "array"
                                && field.Schema.Custom == "com.atlassian.jira.plugin.system.customfieldtypes:multicheckboxes")
                .Where(field => context.SearchString == null ||
                                field.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));

            return customCheckboxFields.ToDictionary(field => field.Id, field => field.Name);
        }
    }
}