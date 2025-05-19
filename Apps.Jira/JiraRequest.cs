using RestSharp;

namespace Apps.Jira;

public class JiraRequest(string endpoint, Method method) : RestRequest(endpoint, method)
{ }