using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;
using System.Text.Json;

namespace Apps.Jira.Auth.OAuth2
{
    public class OAuth2TokenService : BaseInvocable, IOAuth2TokenService
    {
        private const string AtlassianTokenUrl = "https://auth.atlassian.com/oauth/token";
        private const string ExpiresAtKeyName = "expires_at";

        public OAuth2TokenService(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        public bool IsRefreshToken(Dictionary<string, string> values) 
            => values.TryGetValue(ExpiresAtKeyName, out var expireValue) && DateTime.UtcNow > DateTime.Parse(expireValue);

        public async Task<Dictionary<string, string>> RefreshToken(Dictionary<string, string> values,
            CancellationToken cancellationToken)
        {
            const string grantType = "refresh_token";
            var bodyParameters = new Dictionary<string, string>
            {
                { "grant_type", grantType },
                { "client_id", ApplicationConstants.ClientId },
                { "client_secret", ApplicationConstants.ClientSecret },
                { "refresh_token", values["refresh_token"] }
            };
            return await RequestToken(bodyParameters, cancellationToken);
        }

        public async Task<Dictionary<string, string?>> RequestToken(
            string state, 
            string code, 
            Dictionary<string, string> values, 
            CancellationToken cancellationToken)
        {
            var bodyParameters = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", ApplicationConstants.ClientId },
                { "client_secret", ApplicationConstants.ClientSecret },
                { "redirect_uri", $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/AuthorizationCode" },
                { "code", code }
            };
            return await RequestToken(bodyParameters, cancellationToken);
        }

        public Task RevokeToken(Dictionary<string, string> values)
        {
            throw new NotImplementedException();
        }

        private async Task<Dictionary<string, string>> RequestToken(Dictionary<string, string> bodyParameters,
            CancellationToken cancellationToken)
        {
            var utcNow = DateTime.UtcNow;
            using HttpClient httpClient = new HttpClient();
            using var httpContent = new FormUrlEncodedContent(bodyParameters);
            using var response = await httpClient.PostAsync(AtlassianTokenUrl, httpContent, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var resultDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent)?
                                       .ToDictionary(r => r.Key, r => r.Value?.ToString())
                ?? throw new InvalidOperationException($"Invalid response content: {responseContent}");
            var expiresIn = int.Parse(resultDictionary["expires_in"]);
            var expiresAt = utcNow.AddSeconds(expiresIn);
            resultDictionary.Add(ExpiresAtKeyName, expiresAt.ToString());
            return resultDictionary;
        }
    }
}