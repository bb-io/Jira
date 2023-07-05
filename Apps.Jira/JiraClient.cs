using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;

namespace Apps.Jira
{
    public class JiraClient : RestClient
    {
        private static Uri GetUri(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            var cloudId = GetJiraCloudId(authenticationCredentialsProviders);
            var uri = $"https://api.atlassian.com/ex/jira/{cloudId}/rest/api/3";
            return new Uri(uri);
        }

        private static string GetJiraCloudId(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            const string atlassianResourcesUrl = "https://api.atlassian.com/oauth/token/accessible-resources";
            string jiraUrl = authenticationCredentialsProviders.First(p => p.KeyName == "jira_url").Value;
            string authorizationHeader = authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value;
            var restClient = new RestClient(new RestClientOptions { ThrowOnAnyError = true, BaseUrl = new Uri(atlassianResourcesUrl) });
            var request = new RestRequest("", Method.Get);
            request.AddHeader("Authorization", authorizationHeader);
            var atlassianCloudResources = restClient.Get<List<AtlassianCloudResourceDto>>(request);
            var cloudId = atlassianCloudResources.First(jiraResource => jiraUrl.Contains(jiraResource.Url)).Id
                          ?? throw new ArgumentException("The Jira URL is incorrect.");
            return cloudId;
        }

        public JiraClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) 
            : base(new RestClientOptions()
            {
                ThrowOnAnyError = true, BaseUrl = GetUri(authenticationCredentialsProviders)
            }) { }
    }
}
