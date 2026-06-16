using Apps.Jira.Contants;
using Apps.Jira.Dtos;
using Apps.Jira.Extensions;
using Apps.Jira.Models.Utility;
using Apps.Jira.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.Jira.Auth.OAuth2;

public class OAuth2TokenService(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IOAuth2TokenService, ITokenRefreshable
{
    private const string AtlassianTokenUrl = "https://auth.atlassian.com/oauth/token";
    private const string ExpiresAtKeyName = "expires_at";
    private const int RefreshBufferMinutes = 30;

    public bool IsRefreshToken(Dictionary<string, string> values)
    {
        if (!values.TryGetValue(ExpiresAtKeyName, out var expireValue))
            return false;

        return DateTime.TryParse(expireValue, out var expiresAt) && DateTime.UtcNow > expiresAt;
    }

    public int? GetRefreshTokenExprireInMinutes(Dictionary<string, string> values)
    {
        if (!values.TryGetValue(ExpiresAtKeyName, out var expireValue))
            return null;

        if (!DateTime.TryParse(expireValue, out var expireDate))
            return null;

        var difference = expireDate - DateTime.UtcNow;

        return (int)difference.TotalMinutes;
    }

    public async Task<Dictionary<string, string>> RefreshToken(
        Dictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        if (!values.TryGetValue("refresh_token", out var refreshToken))
            throw new InvalidOperationException("Refresh token not found in authentication values");

        if (!values.TryGetValue(CredNames.JiraUrl, out var jiraUrl))
            throw new InvalidOperationException("Jira URL not found in authentication values");

        var creds = OAuth2Credentials.Create(values);
        var bodyParameters = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = creds.ClientId,
            ["client_secret"] = creds.ClientSecret,
            ["refresh_token"] = refreshToken
        };

        InvocationContext.Logger?.LogInformation(
            $"[Jira][OAuth] Starting refresh token flow. JiraUrl: {jiraUrl}; RefreshToken: {GetTokenFingerprint(refreshToken)}; LocalExpiresAt: {GetSafeValue(values, ExpiresAtKeyName)}; MinutesUntilLocalExpiry: {GetRefreshTokenExprireInMinutes(values)?.ToString() ?? "n/a"}",
            null);

        return await FetchOAuthTokenAsync(bodyParameters, jiraUrl, cancellationToken);
    }

    public async Task<Dictionary<string, string>> RequestToken(
        string state,
        string code,
        Dictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        var bridgeServiceUrl = InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/');
        var redirectUri = $"{bridgeServiceUrl}/AuthorizationCode";

        if (!values.TryGetValue(CredNames.JiraUrl, out var jiraUrl))
            throw new InvalidOperationException("Jira URL not found in values");

        var creds = OAuth2Credentials.Create(values);
        var bodyParameters = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = creds.ClientId,
            ["client_secret"] = creds.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["code"] = code
        };

        return await FetchOAuthTokenAsync(bodyParameters, jiraUrl, cancellationToken);
    }

    public Task RevokeToken(Dictionary<string, string> values)
    {
        throw new NotImplementedException();
    }

    private async Task<Dictionary<string, string>> FetchOAuthTokenAsync(
        Dictionary<string, string> bodyParameters,
        string jiraUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = new RestClient(AtlassianTokenUrl);
            var request = new RestRequest(string.Empty, Method.Post);
            
            foreach (var (key, value) in bodyParameters)
                request.AddParameter(key, value);

            var grantType = bodyParameters.TryGetValue("grant_type", out var currentGrantType)
                ? currentGrantType
                : "unknown";
            var refreshTokenFingerprint = bodyParameters.TryGetValue("refresh_token", out var currentRefreshToken)
                ? GetTokenFingerprint(currentRefreshToken)
                : "n/a";

            InvocationContext.Logger?.LogInformation(
                $"[Jira][OAuth] Requesting OAuth token. GrantType: {grantType}; JiraUrl: {jiraUrl}; RefreshToken: {refreshTokenFingerprint}",
                null);

            var response = await client.ExecutePostAsync(request, cancellationToken);
             
            if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
            {
                InvocationContext.Logger?.LogWarning(
                    $"[Jira][OAuth] OAuth token request failed. GrantType: {grantType}; JiraUrl: {jiraUrl}; StatusCode: {(int)response.StatusCode} {response.StatusCode}; Error: {response.ErrorMessage ?? response.Content}",
                    null);
                InvocationContext.Logger?.LogInformation(
                    $"[Jira][OAuth] OAuth token response body. GrantType: {grantType}; JiraUrl: {jiraUrl}; Body: {SanitizeOAuthResponseBody(response.Content)}",
                    null);

                throw new PluginApplicationException(
                    $"Failed to obtain OAuth token: {response.StatusCode}. {response.ErrorMessage ?? response.Content}");
            }

            var tokenResponse = response.Content.Deserialize<OAuth2TokenResponseDto>();

            InvocationContext.Logger?.LogInformation(
                $"[Jira][OAuth] OAuth token response body. GrantType: {grantType}; JiraUrl: {jiraUrl}; Body: {SanitizeOAuthResponseBody(response.Content)}",
                null);
             
            if (tokenResponse == null)
                throw new InvalidOperationException("Failed to deserialize OAuth token response");

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                throw new InvalidOperationException("OAuth response did not contain an access token");

            var utcNow = DateTime.UtcNow;
            var expiresAt = utcNow.AddMinutes(-RefreshBufferMinutes).AddSeconds(tokenResponse.ExpiresIn);

            if (tokenResponse.ExpiresIn < RefreshBufferMinutes * 60)
            {
                InvocationContext.Logger?.LogWarning(
                    $"[Jira][OAuth] OAuth token lifetime is shorter than the configured refresh buffer. GrantType: {grantType}; JiraUrl: {jiraUrl}; ExpiresInSeconds: {tokenResponse.ExpiresIn}; RefreshBufferMinutes: {RefreshBufferMinutes}",
                    null);
            }

            var cloudId = await CloudIdHelper.GetCloudIdAsync(tokenResponse.AccessToken, jiraUrl, cancellationToken);
            var nextRefreshToken = string.IsNullOrWhiteSpace(tokenResponse.RefreshToken)
                ? currentRefreshToken ?? string.Empty
                : tokenResponse.RefreshToken;

            InvocationContext.Logger?.LogInformation(
                $"[Jira][OAuth] OAuth token request succeeded. GrantType: {grantType}; JiraUrl: {jiraUrl}; ExpiresInSeconds: {tokenResponse.ExpiresIn}; RefreshBufferMinutes: {RefreshBufferMinutes}; LocalExpiresAt: {expiresAt:O}; NewRefreshToken: {GetTokenFingerprint(tokenResponse.RefreshToken)}; CloudIdResolved: {!string.IsNullOrWhiteSpace(cloudId)}",
                null);
            InvocationContext.Logger?.LogInformation(
                $"[Jira][OAuth] Refresh token rotation. GrantType: {grantType}; JiraUrl: {jiraUrl}; PreviousRefreshToken: {refreshTokenFingerprint}; NextRefreshToken: {GetTokenFingerprint(nextRefreshToken)}; RefreshTokenReturned: {!string.IsNullOrWhiteSpace(tokenResponse.RefreshToken)}",
                null);

            return new Dictionary<string, string>
            {
                ["access_token"] = tokenResponse.AccessToken,
                // Keep the previous refresh token when the provider omits it to avoid overwriting a valid token with an empty string.
                ["refresh_token"] = nextRefreshToken,
                ["expires_in"] = tokenResponse.ExpiresIn.ToString(),
                ["token_type"] = tokenResponse.TokenType,
                [ExpiresAtKeyName] = expiresAt.ToString("O"),
                [CredNames.CloudId] = cloudId,
                [CredNames.JiraUrl] = jiraUrl
            };
        }
        catch (Exception ex) when (ex is not PluginApplicationException && ex is not PluginMisconfigurationException && ex is not InvalidOperationException)
        {
            InvocationContext.Logger?.LogWarning(
                $"[Jira][OAuth] Unexpected error during OAuth token fetch. JiraUrl: {jiraUrl}; Error: {ex.Message}",
                null);

            throw new PluginApplicationException($"Unexpected error during OAuth token fetch: {ex.Message}", ex);
        }
    }

    private static string GetSafeValue(Dictionary<string, string> values, string key)
        => values.TryGetValue(key, out var value) ? value : "n/a";

    private static string GetTokenFingerprint(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return "missing";

        return token.Length <= 8
            ? $"len:{token.Length}"
            : $"len:{token.Length};suffix:{token[^6..]}";
    }

    private static string SanitizeOAuthResponseBody(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "empty";

        try
        {
            var json = JToken.Parse(content);

            if (json is JObject obj)
            {
                MaskToken(obj, "access_token");
                MaskToken(obj, "refresh_token");
                MaskToken(obj, "id_token");
            }

            return json.ToString(Newtonsoft.Json.Formatting.None);
        }
        catch
        {
            return content;
        }
    }

    private static void MaskToken(JObject obj, string key)
    {
        if (obj.TryGetValue(key, out var tokenValue) && tokenValue.Type == JTokenType.String)
            obj[key] = GetTokenFingerprint(tokenValue.Value<string>());
    }
}
