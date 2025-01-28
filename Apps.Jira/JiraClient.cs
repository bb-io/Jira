using System.Text;
using Apps.Jira.Dtos;
using Apps.Jira.Extensions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Newtonsoft.Json.Linq;
using Polly.Retry;
using RestSharp;

namespace Apps.Jira
{
    public class JiraClient : RestClient
    {
        private readonly AsyncRetryPolicy<RestResponse> _retryPolicy;

        public JiraClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, string routeType = "api")
            : base(new RestClientOptions
            { ThrowOnAnyError = false, BaseUrl = GetUri(authenticationCredentialsProviders, routeType) })
        {
            this.AddDefaultHeader("Authorization",
                authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value);

            _retryPolicy = JiraPollyPolicies.GetTooManyRequestsRetryPolicy();
        }

        public async Task<T> ExecuteWithHandling<T>(RestRequest request)
        {
            var response = await ExecuteWithHandling(request);

            return response.Content.Deserialize<T>();
        }

        public async Task<RestResponse> ExecuteWithHandling(RestRequest request)
        {
            var response = await _retryPolicy.ExecuteAsync(() => base.ExecuteAsync(request));

            if (response.IsSuccessful)
                return response;

            throw ConfigureErrorException(response);
        }

        private static Uri GetUri(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, string routeType)
        {
            var cloudId = GetJiraCloudId(authenticationCredentialsProviders);

            string basePath = routeType switch
            {
                "agile" => $"/rest/agile/1.0",
                "api" => $"/rest/api/3",
                _ => throw new ArgumentException("Invalid route type")
            };

            var uri = $"https://api.atlassian.com/ex/jira/{cloudId}{basePath}";
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
            try
            {
                var error = response.Content.Deserialize<ErrorDto>();
                var errorMessages = string.Join(" ", error.ErrorMessages);
                if (string.IsNullOrEmpty(errorMessages))
                {
                    var errorData = error.Errors.Values().Select(x => x.ToString());
                    if (errorData.Any())
                    {
                        throw new PluginApplicationException(errorData.First());
                    }
                    throw new PluginApplicationException("Internal system error");
                }
                throw new PluginApplicationException(errorMessages);
            }
            catch (Exception ex)
            {
                throw new PluginApplicationException(ex.Message);
            }
        }
    }
}
