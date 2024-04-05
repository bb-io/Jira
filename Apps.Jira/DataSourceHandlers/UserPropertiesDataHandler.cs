using Apps.Jira.Actions;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Jira.DataSourceHandlers;

public class UserPropertiesDataHandler : JiraInvocable, IAsyncDataSourceHandler
{
    private readonly string _accountId;
    
    public UserPropertiesDataHandler(InvocationContext invocationContext, [ActionParameter] GetUserPropertyRequest request) : base(invocationContext)
    {
        _accountId = request.AccountId;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var userPropertiesAction = new UserPropertiesAction(InvocationContext);
        
        var response = await userPropertiesAction.GetUserProperties(new UserIdentifier { AccountId = _accountId });
        return response.Keys.ToDictionary(p => p.Key, p => p.Key);
    }
}