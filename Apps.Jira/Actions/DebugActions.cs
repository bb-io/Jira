using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Jira.Actions;

[ActionList]
public class DebugActions(InvocationContext invocationContext) : JiraInvocable(invocationContext)
{
    [Action("DEBUG: Get auth data", Description = "Can be used only for debugging purposes.")]
    public List<AuthenticationCredentialsProvider> GetAuthenticationCredentialsProviders()
    {
        return InvocationContext.AuthenticationCredentialsProviders.ToList();
    }
}