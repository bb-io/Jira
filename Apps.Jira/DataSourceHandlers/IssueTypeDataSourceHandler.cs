using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
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
        
        var getProjectRequest = new JiraRequest($"/project/{_projectIdentifier.ProjectKey}", Method.Get);
        var project = await Client.ExecuteWithHandling<ProjectDto>(getProjectRequest);
        
        var getIssueTypesRequest = new JiraRequest("/issuetype", Method.Get);
        var issueTypes = await Client.ExecuteWithHandling<IEnumerable<IssueTypeDto>>(getIssueTypesRequest);
        return issueTypes
            .Where(type => type.Scope?.Type == "PROJECT" && type.Scope.Project!.Id == project.Id)
            .Where(type => context.SearchString == null 
                           || type.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(type => type.Id, type => type.Name);
    }
}