using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Microsoft.AspNetCore.WebUtilities;

namespace Apps.Jira.Auth.OAuth2
{
    public class OAuth2AuthorizeService : IOAuth2AuthorizeService
    {
        public string GetAuthorizationUrl(Dictionary<string, string> values)
        {
            const string atlassianAuthorizeUrl = "https://auth.atlassian.com/authorize";
            var parameters = new Dictionary<string, string>
            {
                { "audience", "api.atlassian.com" },
                { "client_id", values["client_id"] },
                { "scope", "read:jira-work write:jira-work manage:jira-webhook read:jira-user" },
                { "redirect_uri", ApplicationConstants.RedirectUri },
                { "state", values["state"] },
                { "response_type", "code" },
                { "prompt", "consent" }
            };
            return QueryHelpers.AddQueryString(atlassianAuthorizeUrl, parameters);
        }
    }
}