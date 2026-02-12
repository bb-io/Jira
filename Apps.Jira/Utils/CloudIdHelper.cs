using Apps.Jira.Dtos;
using Apps.Jira.Extensions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using RestSharp;

namespace Apps.Jira.Utils;

public static class CloudIdHelper
{
    private const string AtlassianResourcesUrl = "https://api.atlassian.com/oauth/token/accessible-resources";

    public static async Task<string> GetCloudIdAsync(
        string accessToken,
        string jiraUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("Access token cannot be null or empty", nameof(accessToken));

        if (string.IsNullOrWhiteSpace(jiraUrl))
            throw new PluginMisconfigurationException("Jira URL is not configured");

        try
        {
            var client = new RestClient(AtlassianResourcesUrl);
            var request = new RestRequest(string.Empty, Method.Get);
            request.AddHeader("Authorization", $"Bearer {accessToken}");

            var response = await client.ExecuteAsync(request, cancellationToken);

            if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
            {
                throw new PluginApplicationException(
                    $"Failed to fetch accessible resources: {response.StatusCode}. {response.ErrorMessage ?? response.Content}");
            }

            var resources = response.Content.Deserialize<List<AtlassianCloudResourceDto>>();

            if (resources == null)
                throw new InvalidOperationException("Failed to deserialize Atlassian resources");

            var matchingResource = resources.FirstOrDefault(r =>
                !string.IsNullOrWhiteSpace(r.Url) && jiraUrl.Contains(r.Url, StringComparison.OrdinalIgnoreCase));

            if (matchingResource == null || string.IsNullOrWhiteSpace(matchingResource.Id))
            {
                throw new PluginMisconfigurationException(
                    $"No matching Atlassian Cloud resource found for Jira URL: {jiraUrl}");
            }

            return matchingResource.Id;
        }
        catch (Exception ex) when (ex is not PluginApplicationException && 
                                     ex is not PluginMisconfigurationException && 
                                     ex is not ArgumentException)
        {
            throw new PluginApplicationException($"Unexpected error fetching Jira Cloud ID: {ex.Message}", ex);
        }
    }
    
    public static string GetCloudId(string accessToken, string jiraUrl)
    {
        return GetCloudIdAsync(accessToken, jiraUrl, CancellationToken.None).GetAwaiter().GetResult();
    }
}
