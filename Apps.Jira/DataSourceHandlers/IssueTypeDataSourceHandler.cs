using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers;

public class IssueTypeDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    private readonly ProjectIdentifier _projectIdentifier;

    public IssueTypeDataSourceHandler(InvocationContext invocationContext,
        [ActionParameter] ProjectIdentifier projectIdentifier) : base(invocationContext)
    {
        _projectIdentifier = projectIdentifier;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (_projectIdentifier.ProjectKey == null)
        throw new Exception("Please specify project key first.");

    var request = new JiraRequest($"/issue/createmeta?projectKeys={_projectIdentifier.ProjectKey}&expand=projects.issuetypes", Method.Get);
    var response = await Client.ExecuteWithHandling<CreateMetaDto>(request);

    return response.Projects
        .SelectMany(p => p.IssueTypes)
        .Where(type => context.SearchString == null || type.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
        .ToDictionary(type => type.Id, type => type.Name);
    }
}