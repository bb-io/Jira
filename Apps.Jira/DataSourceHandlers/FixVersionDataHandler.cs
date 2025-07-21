using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers;

public class FixVersionDataHandler(InvocationContext invocationContext, [ActionParameter] ProjectIdentifier projectIdentifier)
    : JiraInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    private readonly ProjectIdentifier _projectIdentifier = projectIdentifier;

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var request = new JiraRequest($"/project/{_projectIdentifier.ProjectKey}/versions", Method.Get);
        var versions = await Client.ExecuteWithHandling<List<FixVersionDto>>(request);

        return versions
            .Where(x => context.SearchString == null
                        || x.VersionName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(x => new DataSourceItem(x.VersionName, x.VersionName));
    }
}