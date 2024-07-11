using System.Text;
using Apps.Jira.Dtos;
using Apps.Jira.Extensions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.Jira
{
    public class JiraClient : RestClient
    {
        public JiraClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
            : base(new RestClientOptions
                { ThrowOnAnyError = false, BaseUrl = GetUri(authenticationCredentialsProviders) })
        {
            this.AddDefaultHeader("Authorization", 
                authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value);
        }
        
        public async Task<T> ExecuteWithHandling<T>(RestRequest request)
        {
            var response = await ExecuteWithHandling(request);
            return response.Content.Deserialize<T>();
        }
    
        public async Task<RestResponse> ExecuteWithHandling(RestRequest request)
        {
            var response = await ExecuteAsync(request);
        
            if (response.IsSuccessful)
                return response;

            throw ConfigureErrorException(response);
        }
        
        private static Uri GetUri(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            var cloudId = GetJiraCloudId(authenticationCredentialsProviders);
            var uri = $"https://api.atlassian.com/ex/jira/{cloudId}/rest/api/3";
            return new Uri(uri);
        }

        private static string GetJiraCloudId(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            const string atlassianResourcesUrl = "https://api.atlassian.com/oauth/token/accessible-resources";
            string jiraUrl = authenticationCredentialsProviders.First(p => p.KeyName == "JiraUrl").Value;
            string authorizationHeader = authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value;
            var restClient = new RestClient(new RestClientOptions 
                { ThrowOnAnyError = true, BaseUrl = new Uri(atlassianResourcesUrl) });
            var request = new RestRequest("");
            request.AddHeader("Authorization", authorizationHeader);
            var atlassianCloudResources = restClient.Get<List<AtlassianCloudResourceDto>>(request);
            var cloudId = atlassianCloudResources.First(jiraResource => jiraUrl.Contains(jiraResource.Url)).Id
                          ?? throw new ArgumentException("The Jira URL is incorrect.");
            return cloudId;
        }
        
        private Exception ConfigureErrorException(RestResponse response)
        {
            var error = response.Content.Deserialize<ErrorDto>();
            var errorMessages = string.Join(" ", error.ErrorMessages);
            if (string.IsNullOrEmpty(errorMessages))
            {
                var errorData = error.Errors.Values().Select(x => x.ToString());
                if(errorData.Any())
                {
                    return new(errorData.First());
                }
                return new("Internal system error");
            }
            return new(errorMessages);
        }
    }
}
