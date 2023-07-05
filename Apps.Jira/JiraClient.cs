using System.Text.Json;
using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;

namespace Apps.Jira
{
    public class JiraClient : RestClient
    {
        private static Uri GetUri(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            var cloudId = GetJiraCloudId(authenticationCredentialsProviders).Result;
            var uri = $"https://api.atlassian.com/ex/jira/{cloudId}/rest/api/3";
            return new Uri(uri);
        }

        private static async Task<string> GetJiraCloudId(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            const string atlassianResourcesUrl = "https://api.atlassian.com/oauth/token/accessible-resources";
            string jiraUrl = authenticationCredentialsProviders.First(p => p.KeyName == "jira_url").Value;
            string authorizationHeader =
                    authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value;
            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
            using var response = await httpClient.GetAsync(atlassianResourcesUrl);
            var responseContent = await response.Content.ReadAsStringAsync();
            var atlassianCloudResources = JsonSerializer.Deserialize<List<AtlassianCloudResourceDto>>(responseContent) 
                                ?? throw new InvalidOperationException($"Invalid response content: {responseContent}"); ;
            var cloudId = atlassianCloudResources.First(jiraResource => jiraUrl.Contains(jiraResource.Url)).Id;
            return cloudId;
        }

        public JiraClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) 
            : base(new RestClientOptions()
            {
                ThrowOnAnyError = true, BaseUrl = GetUri(authenticationCredentialsProviders)
            }) { }
    }
}
