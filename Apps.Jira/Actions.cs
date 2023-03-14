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

namespace Apps.Jira
{
    [ActionList]
    public class Actions
    {
        [Action("Get issue", Description = "Get issue by key")]
        public IssueResponse GetIssueByKey(string url, string login, AuthenticationCredentialsProvider authenticationCredentialsProvider, 
            [ActionParameter] IssueRequest input)
        {
            var fields = new List<string>() { "summary", "status", "assignee" };
            var request = CreateRequestToJira(login, authenticationCredentialsProvider.Value, $"/rest/api/3/issue/{input.IssueKey}/?fields={string.Join(",", fields)}", Method.Get);
            var response = new RestClient(url).Get(request);

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
        public BaseResponse IssueTransition(string url, string login, AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] IssueTransitionRequest input)
        {
            var request = CreateRequestToJira(login, authenticationCredentialsProvider.Value, $"/rest/api/3/issue/{input.IssueKey}/transitions", Method.Post);
            request.AddJsonBody(new
            {
                transition = new
                {
                    id = input.TransitionId
                }
            });
            var response = new RestClient(url).Execute(request);

            return new BaseResponse()
            {
                StatusCode = ((int)response.StatusCode),
                Details = response.Content
            };
        }

        [Action("Get issue transitions", Description = "Get list of available transitions for specific issue")]
        public GetIssueTransitionsResponse GetIssueTransitions(string url, string login, AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] GetIssueTransitionsRequest input)
        {
            var request = CreateRequestToJira(login, authenticationCredentialsProvider.Value, $"/rest/api/3/issue/{input.IssueKey}/transitions", Method.Get);
            var response = new RestClient(url).Get(request);

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
        public GetAllUsersResponse GetAllUsers(string url, string login, AuthenticationCredentialsProvider authenticationCredentialsProvider)
        {
            var request = CreateRequestToJira(login, authenticationCredentialsProvider.Value, $"/rest/api/3/users/search", Method.Get);
            var response = new RestClient(url).Get(request);

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

        private RestRequest CreateRequestToJira(string email, string token, string endpoint,
            RestSharp.Method methodType)
        {
            string base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{token}"));
            var request = new RestRequest(endpoint, methodType);
            request.AddHeader("Authorization", $"Basic {base64Key}");
            return request;
        }
    }
}
