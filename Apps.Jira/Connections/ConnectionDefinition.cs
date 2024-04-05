using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.Jira.Connections
{
    public class ConnectionDefinition : IConnectionDefinition
    {
        public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>
        {
            new ConnectionPropertyGroup
            {
                Name = "OAuth2",
                AuthenticationType = ConnectionAuthenticationType.OAuth2,
                ConnectionUsage = ConnectionUsage.Actions,
                ConnectionProperties = new List<ConnectionProperty>
                {
                    new("Jira URL")
                }
            },
        };

        public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
            Dictionary<string, string> values)
        {
            var token = values.First(v => v.Key == "access_token");
            yield return new AuthenticationCredentialsProvider(
                AuthenticationCredentialsRequestLocation.Header,
                "Authorization",
                $"Bearer {token.Value}"
            );
            
            var jiraUrl = new Uri(values.First(v => v.Key == "Jira URL").Value).GetLeftPart(UriPartial.Authority);
            yield return new AuthenticationCredentialsProvider(
                AuthenticationCredentialsRequestLocation.None,
                "JiraUrl",
                jiraUrl
            );
        }
    }
}
