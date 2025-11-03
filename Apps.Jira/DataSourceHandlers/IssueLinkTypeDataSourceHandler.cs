using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers
{
    public class IssueLinkTypeDataSourceHandler(InvocationContext invocationContext) : JiraInvocable(invocationContext), IAsyncDataSourceItemHandler
    {
        public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
        {
            var request = new JiraRequest("/issueLinkType", Method.Get);
            var response = await Client.ExecuteWithHandling<IssueLinkTypesWrapper>(request);
            return response.IssueLinkTypes.Select(linkType => new DataSourceItem(linkType.Name, $"{linkType.Name}: {linkType.Inward}/{linkType.Outward}"));
        }
    }
}
