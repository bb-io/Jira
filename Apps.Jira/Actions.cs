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
        public IssueResponse GetIssueByKey(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
            [ActionParameter] IssueRequest input)
        {
            var fields = new List<string>() { "summary", "status", "assignee" };
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest($"/issue/{input.IssueKey}/?fields={string.Join(",", fields)}", Method.Get, authenticationCredentialsProviders);

            var response = client.Get(request);

            dynamic parsedIssue = JsonConvert.DeserializeObject(response.Content);

            string summary = parsedIssue.fields.summary;
            string status = parsedIssue.fields.status.name;
            string assignee = parsedIssue.fields.assignee.emailAddress;

            return new IssueResponse()
            {
                Summary = summary,
                Status = status,
                Assignee = assignee
            };
        }

        [Action("Issue transition", Description = "Perform issue transition")]
        public BaseResponse IssueTransition(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
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
            var response = client.Execute(request);

            return new BaseResponse()
            {
                StatusCode = ((int)response.StatusCode),
                Details = response.Content
            };
        }

        [Action("Get issue transitions", Description = "Get list of available transitions for specific issue")]
        public GetIssueTransitionsResponse GetIssueTransitions(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] GetIssueTransitionsRequest input)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest($"/issue/{input.IssueKey}/transitions", Method.Get, authenticationCredentialsProviders);
            var response = client.Get(request);

            dynamic availableTransitions = JsonConvert.DeserializeObject(response.Content);
            JArray transitionsArr = availableTransitions.transitions;

            var transitions = new List<TransitionDto>();
            foreach( JObject transition in transitionsArr)
            {
                transitions.Add(new TransitionDto()
                {
                    Name = transition.GetValue("name").ToString(),
                    Id = transition.GetValue("id").ToString()
                });
            }

            return new GetIssueTransitionsResponse()
            {
                Transitions = transitions
            };
        }

        [Action("Get all users", Description = "Get list of users")]
        public GetAllUsersResponse GetAllUsers(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest($"/users/search", Method.Get, authenticationCredentialsProviders);            
            var response = client.Get(request);

            dynamic usersObj = JsonConvert.DeserializeObject(response.Content);
            JArray usersArr = usersObj;

            var users = new List<UserDto>();
            foreach (JObject user in usersArr)
            {
                users.Add(new UserDto()
                {
                    DisplayName = user.GetValue("displayName").ToString(),
                    AccountId = user.GetValue("accountId").ToString()
                });
            }

            return new GetAllUsersResponse()
            {
                Users = users
            };
        }
    }
}
