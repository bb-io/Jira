using Apps.Jira.Contants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.Jira.Connections
{
    public class ConnectionDefinition : IConnectionDefinition
    {
        public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups =>
        [
            new()
            {
                Name = "OAuth2",
                AuthenticationType = ConnectionAuthenticationType.OAuth2,
                ConnectionProperties =
                [
                    new(CredNames.JiraUrl)
                    {
                        DisplayName = "Jira URL",
                        Sensitive = false
                    }
                ]
            },
        ];

        public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
            Dictionary<string, string> values)
        {
            var token = values.First(v => v.Key == "access_token");
            yield return new AuthenticationCredentialsProvider("Authorization", $"Bearer {token.Value}");
            
            var jiraUrl = new Uri(values.First(v => v.Key == CredNames.JiraUrl).Value).GetLeftPart(UriPartial.Authority);
            yield return new AuthenticationCredentialsProvider("JiraUrl", jiraUrl);
            
            var cloudId = values.First(v => v.Key == CredNames.CloudId).Value;
            yield return new AuthenticationCredentialsProvider(CredNames.CloudId, cloudId);
        }
    }
}
