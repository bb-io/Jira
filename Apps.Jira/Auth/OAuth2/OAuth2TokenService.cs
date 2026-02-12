using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.Jira.Contants;
using Apps.Jira.Dtos;
using Apps.Jira.Extensions;
using Apps.Jira.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Exceptions;
using RestSharp;

namespace Apps.Jira.Auth.OAuth2;

public class OAuth2TokenService(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IOAuth2TokenService
{
    private const string AtlassianTokenUrl = "https://auth.atlassian.com/oauth/token";
    private const string AtlassianResourcesUrl = "https://api.atlassian.com/oauth/token/accessible-resources";
    private const string ExpiresAtKeyName = "expires_at";

    public bool IsRefreshToken(Dictionary<string, string> values)
    {
        if (!values.TryGetValue(ExpiresAtKeyName, out var expireValue))
            return false;

        return DateTime.TryParse(expireValue, out var expiresAt) && DateTime.UtcNow > expiresAt;
    }

    public async Task<Dictionary<string, string>> RefreshToken(
        Dictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        if (!values.TryGetValue("refresh_token", out var refreshToken))
            throw new InvalidOperationException("Refresh token not found in authentication values");

        var bodyParameters = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = ApplicationConstants.ClientId,
            ["client_secret"] = ApplicationConstants.ClientSecret,
            ["refresh_token"] = refreshToken
        };

        return await FetchOAuthTokenAsync(bodyParameters, cancellationToken);
    }

    public async Task<Dictionary<string, string>> RequestToken(
        string state,
        string code,
        Dictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        var bridgeServiceUrl = InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/');
        var redirectUri = $"{bridgeServiceUrl}/AuthorizationCode";

        var bodyParameters = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = ApplicationConstants.ClientId,
            ["client_secret"] = ApplicationConstants.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["code"] = code
        };

        return await FetchOAuthTokenAsync(bodyParameters, cancellationToken);
    }

    public Task RevokeToken(Dictionary<string, string> values)
    {
        throw new NotImplementedException();
    }

    private async Task<Dictionary<string, string>> FetchOAuthTokenAsync(
        Dictionary<string, string> bodyParameters,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = new RestClient(AtlassianTokenUrl);
            var request = new RestRequest(string.Empty, Method.Post);
            
            foreach (var (key, value) in bodyParameters)
                request.AddParameter(key, value);

            var response = await client.ExecutePostAsync(request, cancellationToken);
            
            if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
            {
                await WebhookLogger.LogErrorAsync(
                    "OAuth2TokenService.FetchOAuthTokenAsync",
                    "Failed to obtain OAuth token",
                    response.ErrorException,
                    new
                    {
                        StatusCode = response.StatusCode,
                        ErrorMessage = response.ErrorMessage,
                        ResponseContent = response.Content,
                        BodyParameters = WebhookLogger.RedactSensitiveData(bodyParameters)
                    });

                throw new PluginApplicationException(
                    $"Failed to obtain OAuth token: {response.StatusCode}. {response.ErrorMessage ?? response.Content}");
            }

            var tokenResponse = response.Content.Deserialize<OAuth2TokenResponseDto>();
            
            if (tokenResponse == null)
            {
                await WebhookLogger.LogErrorAsync(
                    "OAuth2TokenService.FetchOAuthTokenAsync",
                    "Failed to deserialize OAuth token response",
                    null,
                    new
                    {
                        ResponseContent = response.Content,
                        BodyParameters = WebhookLogger.RedactSensitiveData(bodyParameters)
                    });
                    
                throw new InvalidOperationException("Failed to deserialize OAuth token response");
            }

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                await WebhookLogger.LogErrorAsync(
                    "OAuth2TokenService.FetchOAuthTokenAsync",
                    "OAuth response did not contain an access token",
                    null,
                    new { BodyParameters = WebhookLogger.RedactSensitiveData(bodyParameters) });
                    
                throw new InvalidOperationException("OAuth response did not contain an access token");
            }

            var utcNow = DateTime.UtcNow;
            var expiresAt = utcNow.AddSeconds(tokenResponse.ExpiresIn);
            var cloudId = await GetJiraCloudIdAsync(tokenResponse.AccessToken, cancellationToken);

            return new Dictionary<string, string>
            {
                ["access_token"] = tokenResponse.AccessToken,
                ["refresh_token"] = tokenResponse.RefreshToken,
                ["expires_in"] = tokenResponse.ExpiresIn.ToString(),
                ["token_type"] = tokenResponse.TokenType,
                [ExpiresAtKeyName] = expiresAt.ToString("O"),
                [CredNames.CloudId] = cloudId
            };
        }
        catch (Exception ex) when (ex is not PluginApplicationException && ex is not PluginMisconfigurationException)
        {
            await WebhookLogger.LogErrorAsync(
                "OAuth2TokenService.FetchOAuthTokenAsync",
                "Unexpected exception during OAuth token fetch",
                ex,
                new { BodyParameters = WebhookLogger.RedactSensitiveData(bodyParameters) });
            throw;
        }
    }

    private async Task<string> GetJiraCloudIdAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = new RestClient(AtlassianResourcesUrl);
            var request = new RestRequest(string.Empty, Method.Get);
            request.AddHeader("Authorization", $"Bearer {accessToken}");

            var response = await client.ExecuteAsync(request, cancellationToken);
            
            if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
            {
                await WebhookLogger.LogErrorAsync(
                    "OAuth2TokenService.GetJiraCloudIdAsync",
                    "Failed to fetch accessible resources",
                    response.ErrorException,
                    new
                    {
                        StatusCode = response.StatusCode,
                        ErrorMessage = response.ErrorMessage,
                        ResponseContent = response.Content
                    });

                throw new PluginApplicationException(
                    $"Failed to fetch accessible resources: {response.StatusCode}. {response.ErrorMessage ?? response.Content}");
            }

            var resources = response.Content.Deserialize<List<AtlassianCloudResourceDto>>();
            
            if (resources == null)
            {
                await WebhookLogger.LogErrorAsync(
                    "OAuth2TokenService.GetJiraCloudIdAsync",
                    "Failed to deserialize Atlassian resources",
                    null,
                    new { ResponseContent = response.Content });
                    
                throw new InvalidOperationException("Failed to deserialize Atlassian resources");
            }

            var jiraUrlCred = InvocationContext.AuthenticationCredentialsProviders
                .FirstOrDefault(p => p.KeyName == CredNames.JiraUrl);

            if (jiraUrlCred == null || string.IsNullOrWhiteSpace(jiraUrlCred.Value))
            {
                await WebhookLogger.LogErrorAsync(
                    "OAuth2TokenService.GetJiraCloudIdAsync",
                    "Jira URL is not configured",
                    null,
                    new { AvailableCredentials = InvocationContext.AuthenticationCredentialsProviders.Select(p => p.KeyName).ToList() });
                    
                throw new PluginMisconfigurationException("Jira URL is not configured");
            }

            var jiraUrl = jiraUrlCred.Value;
            var matchingResource = resources.FirstOrDefault(r => 
                !string.IsNullOrWhiteSpace(r.Url) && jiraUrl.Contains(r.Url, StringComparison.OrdinalIgnoreCase));

            if (matchingResource == null || string.IsNullOrWhiteSpace(matchingResource.Id))
            {
                await WebhookLogger.LogErrorAsync(
                    "OAuth2TokenService.GetJiraCloudIdAsync",
                    "No matching Atlassian Cloud resource found",
                    null,
                    new
                    {
                        JiraUrl = jiraUrl,
                        AvailableResources = resources.Select(r => new { r.Id, r.Url }).ToList()
                    });

                throw new PluginMisconfigurationException(
                    $"No matching Atlassian Cloud resource found for Jira URL: {jiraUrl}");
            }

            return matchingResource.Id;
        }
        catch (Exception ex) when (ex is not PluginApplicationException && ex is not PluginMisconfigurationException)
        {
            await WebhookLogger.LogErrorAsync(
                "OAuth2TokenService.GetJiraCloudIdAsync",
                "Unexpected exception during cloud ID fetch",
                ex,
                null);
            throw;
        }
    }
}