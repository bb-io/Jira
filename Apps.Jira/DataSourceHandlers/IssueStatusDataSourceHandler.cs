using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers;

public class IssueStatusDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    private readonly ProjectIdentifier _projectIdentifier;

    public IssueStatusDataSourceHandler(InvocationContext invocationContext, 
        [ActionParameter] ProjectIdentifier projectIdentifier) : base(invocationContext)
    {
        _projectIdentifier = projectIdentifier;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (_projectIdentifier.ProjectKey == null)
            throw new Exception("Please specify project key first.");
        
        var client = new JiraClient(Creds);
        var request = new JiraRequest($"/project/{_projectIdentifier.ProjectKey}/statuses", Method.Get,
            Creds);
        var response = await client.ExecuteWithHandling<IEnumerable<StatusesWrapper>>(request);
        
        return response
            .SelectMany(statuses => statuses.Statuses)
            .DistinctBy(status => status.Id)
            .OrderBy(status => status.Id)
            .ToDictionary(status => status.Id, status => status.Name);
    }
}