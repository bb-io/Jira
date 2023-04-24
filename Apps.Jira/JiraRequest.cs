using Blackbird.Applications.Sdk.Common.Authentication;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Jira
{
    public class JiraRequest : RestRequest
    {
        public JiraRequest(string endpoint, Method method, IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) : base(endpoint, method)
        {
            var email = authenticationCredentialsProviders.First(p => p.KeyName == "email").Value;
            var apiKey = authenticationCredentialsProviders.First(p => p.KeyName == "apiKey").Value;

            string base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{apiKey}"));
            this.AddHeader("Authorization", $"Basic {base64Key}");
        }
    }
}
