using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Jira;

public class JiraInvocable : BaseInvocable
{
    protected readonly JiraClient Client;

    protected JiraInvocable(InvocationContext invocationContext) : base(invocationContext)
    {
        Client = new JiraClient(invocationContext.AuthenticationCredentialsProviders);
    }
}