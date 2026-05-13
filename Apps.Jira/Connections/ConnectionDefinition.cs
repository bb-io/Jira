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
                Name = ConnectionTypes.OAuth2,
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
            new()
            {
                Name = ConnectionTypes.OAuth2CustomApp,
                DisplayName = "OAuth2 (custom app)",
                AuthenticationType = ConnectionAuthenticationType.OAuth2,
                ConnectionProperties =
                [
                    new(CredNames.JiraUrl) { DisplayName = "Jira URL" },
                    new(CredNames.ClientId) { DisplayName = "Client ID" },
                    new(CredNames.ClientSecret) { DisplayName = "Secret", Sensitive = true },
                    new(CredNames.CustomScopes) { DisplayName = "Scopes" },
                ]
            },
        ];

        public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
            Dictionary<string, string> values)
        {
            var token = values.First(v => v.Key == "access_token");
            yield return new AuthenticationCredentialsProvider("Authorization", $"Bearer {token.Value}");
            
            var jiraUrl = new Uri(values.First(v => v.Key == CredNames.JiraUrl).Value).GetLeftPart(UriPartial.Authority);
            yield return new AuthenticationCredentialsProvider(CredNames.JiraUrl, jiraUrl);
            
            var connectionType = values[nameof(ConnectionPropertyGroup)] switch
            {
                var ct when ConnectionTypes.SupportedConnectionTypes.Contains(ct) => ct,
                _ => throw new Exception($"Unknown connection type: {values[nameof(ConnectionPropertyGroup)]}")
            };
            yield return new AuthenticationCredentialsProvider(CredNames.ConnectionType, connectionType);
            
            var customKeys = new[] 
            { 
                CredNames.CloudId, 
                CredNames.ClientId, 
                CredNames.ClientSecret, 
                CredNames.CustomScopes 
            };

            foreach (var key in customKeys)
            {
                if (values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                    yield return new AuthenticationCredentialsProvider(key, value);
            }
        }
    }
}
