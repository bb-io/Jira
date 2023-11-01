using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.AspNetCore.WebUtilities;

namespace Apps.Jira.Auth.OAuth2
{
    public class OAuth2AuthorizeService : BaseInvocable, IOAuth2AuthorizeService
    {
        public OAuth2AuthorizeService(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        public string GetAuthorizationUrl(Dictionary<string, string> values)
        {
            string bridgeOauthUrl = $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/oauth";
            const string atlassianAuthorizeUrl = "https://auth.atlassian.com/authorize";
            var parameters = new Dictionary<string, string>
            {
                { "audience", "api.atlassian.com" },
                { "client_id", ApplicationConstants.ClientId },
                { "scope", ApplicationConstants.Scopes },
                { "redirect_uri", $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/AuthorizationCode" },
                { "state", values["state"] },
                { "response_type", "code" },
                { "prompt", "consent" },
                { "authorization_url", bridgeOauthUrl},
                { "actual_redirect_uri", InvocationContext.UriInfo.AuthorizationCodeRedirectUri.ToString() },
            };
            return QueryHelpers.AddQueryString(bridgeOauthUrl, parameters);
        }
    }
}