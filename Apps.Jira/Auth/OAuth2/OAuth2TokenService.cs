using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using System.Text.Json;

namespace Apps.Jira.Auth.OAuth2
{
    public class OAuth2TokenService : IOAuth2TokenService
    {
        private const string AtlassianTokenUrl = "https://auth.atlassian.com/oauth/token";

        public bool IsRefreshToken(Dictionary<string, string> values)
        {
            return false;
        }

        public async Task<Dictionary<string, string>> RefreshToken(Dictionary<string, string> values,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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
                { "redirect_uri", ApplicationConstants.RedirectUri },
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
            using HttpClient httpClient = new HttpClient();
            using var httpContent = new FormUrlEncodedContent(bodyParameters);
            using var response = await httpClient.PostAsync(AtlassianTokenUrl, httpContent, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync();
            var resultDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent)?
                                       .ToDictionary(r => r.Key, r => r.Value?.ToString())
                ?? throw new InvalidOperationException($"Invalid response content: {responseContent}");
            return resultDictionary;
        }
    }
}