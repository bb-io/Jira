using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Jira.Actions;

[ActionList]
public class UserActions : JiraInvocable
{
    protected UserActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }
}