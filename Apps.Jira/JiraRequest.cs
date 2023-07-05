using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;

namespace Apps.Jira
{
    public class JiraRequest : RestRequest
    {
        public JiraRequest(string endpoint, Method method,
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) : base(endpoint, method)
        {
            this.AddHeader("Authorization",
                authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value);
        }
    }
}
