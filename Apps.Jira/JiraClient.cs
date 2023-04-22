using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Jira
{
    public class JiraClient : RestClient
    {
        private static Uri GetUri(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            return new Uri(authenticationCredentialsProviders.First(p => p.KeyName == "url").Value + "/rest/api/3");
        }

        public JiraClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) : base(new RestClientOptions() { ThrowOnAnyError = true, BaseUrl = GetUri(authenticationCredentialsProviders) }) { }
    }
}
