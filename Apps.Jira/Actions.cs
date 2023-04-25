using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using RestSharp;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common.Actions;

namespace Apps.Jira
{
    [ActionList]
    public class Actions
    {
        [Action("Get issue", Description = "Get issue by key")]
        public IssueDto GetIssueByKey(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
            [ActionParameter] IssueRequest input)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest($"/issue/{input.IssueKey}", Method.Get, authenticationCredentialsProviders);
            return client.Get<ResponseWrapper<IssueDto>>(request).Fields;
        }

        [Action("Issue transition", Description = "Perform issue transition")]
        public void IssueTransition(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] IssueTransitionRequest input)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest($"/issue/{input.IssueKey}/transitions", Method.Get, authenticationCredentialsProviders);
            request.AddJsonBody(new
            {
                transition = new
                {
                    id = input.TransitionId
                }
            });
            client.Execute(request);
        }

        [Action("Get issue transitions", Description = "Get list of available transitions for specific issue")]
        public TransitionsResponseWrapper GetIssueTransitions(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] GetIssueTransitionsRequest input)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest($"/issue/{input.IssueKey}/transitions", Method.Get, authenticationCredentialsProviders);
            return client.Get<TransitionsResponseWrapper>(request);
        }

        [Action("Get all users", Description = "Get list of users")]
        public UsersResponseWrapper GetAllUsers(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest($"/users/search", Method.Get, authenticationCredentialsProviders);
            return client.Get<UsersResponseWrapper>(request);
        }
    }
}
