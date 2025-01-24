using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers.CustomFields
{
    public class CustomNumericFieldDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
    {
        public CustomNumericFieldDataSourceHandler(InvocationContext invocationContext) : base(invocationContext) { }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var request = new JiraRequest("/field", Method.Get);
            var fields = await Client.ExecuteWithHandling<IEnumerable<FieldDto>>(request);
            var customStringFields = fields
                .Where(field => field.Custom && field.Schema!.Type == "number")
                .Where(field => context.SearchString == null ||
                                field.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));

            return customStringFields.ToDictionary(field => field.Id, field => field.Name);
        }
    }
}
