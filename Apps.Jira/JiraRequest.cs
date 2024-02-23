using RestSharp;

namespace Apps.Jira
{
    public class JiraRequest : RestRequest
    {
        public JiraRequest(string endpoint, Method method) : base(endpoint, method)
        {
        }
    }
}
