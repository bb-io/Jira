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
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;

namespace Apps.Jira
{
    [ActionList]
    public class Actions
    {
        [Action("Get issue", Description = "Get issue by key")]
        public IssueResponse GetIssueByKey(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
            [ActionParameter] IssueRequest input)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest($"/issue/{input.IssueKey}", Method.Get, authenticationCredentialsProviders);
            var issue = client.Get<ResponseWrapper<IssueDto>>(request).Fields;
            return new IssueResponse()
            {
                Summary = issue.Summary,
                Status = issue.Status.Name,
                Assignee = issue.Assignee == null ? "Unassigned" : issue.Assignee.DisplayName,
                Description = issue.Description == null ? ""
                    : string.Join('\n',
                        issue.Description.Content.Select(x => string.Join('\n', x.Content.Select(c => c.Text)))
                            .ToArray())
            };
        }

        [Action("Issue transition", Description = "Perform issue transition")]
        public void IssueTransition(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] IssueTransitionRequest input)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest($"/issue/{input.IssueKey}/transitions", Method.Post, authenticationCredentialsProviders);
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

        [Action("Create issue", Description = "Create an issue")]
        public async Task CreateIssue(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] CreateIssueRequest input)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest("/issue", Method.Post, authenticationCredentialsProviders);
            request.AddJsonBody(new
            {
                fields = new
                {
                    assignee = new { id = input.AssigneeId },
                    project = new { key = input.ProjectKey },
                    summary = input.Summary,
                    description = new Description()
                    {
                        Version = 1,
                        Type = "doc",
                        Content = new List<ContentObj>()
                        {
                            new ContentObj()
                            {
                                Type = "paragraph", 
                                Content = new List<ContentData>()
                                {
                                    new ContentData()
                                    {
                                        Type = "text",
                                        Text = input.Description
                                    }
                                }
                            }
                        }
                    },
                    issuetype = new { name = input.IssueType }
                }
            });
            await client.ExecuteAsync(request);
        }

        [Action("Assign issue", Description =
            "Assign an issue to a user. Is AccountId is set to -1, issue is assigned to the default assignee for the " +
            "project. Is AccountId is set to null, the issue is set to unassigned")]
        public async Task AssignIssue(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] AssignIssueRequest input)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest($"/issue/{input.IssueKey}/assignee", Method.Put, authenticationCredentialsProviders);
            request.AddJsonBody(new { accountId = input.AccountId });
            await client.ExecuteAsync(request);
        }

        [Action("Update issue summary", Description = "Update summary for an issue")]
        public async Task UpdateIssueSummary(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] UpdateIssueSummaryRequest input)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest($"/issue/{input.IssueKey}", Method.Put, authenticationCredentialsProviders);
            request.AddJsonBody(new
            {
                fields = new { summary = input.Summary }
            });
            await client.ExecuteAsync(request);
        }

        [Action("Update issue description", Description = "Update description for an issue")]
        public async Task UpdateIssueDescription(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] UpdateDescriptionRequest input)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest($"/issue/{input.IssueKey}", Method.Put, authenticationCredentialsProviders);
            request.AddJsonBody(new
            {
                fields = new
                {
                    description = new Description()
                    {
                        Version = 1,
                        Type = "doc",
                        Content = new List<ContentObj>()
                        {
                            new ContentObj()
                            {
                                Type = "paragraph", 
                                Content = new List<ContentData>()
                                {
                                    new ContentData()
                                    {
                                        Type = "text",
                                        Text = input.Description
                                    }
                                }
                            }
                        }
                    }
                }
            });
            await client.ExecuteAsync(request);
        }

        [Action("Prioritize issue", Description = "Set priority for an issue")]
        public async Task PrioritizeIssue(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] PrioritizeIssueRequest input)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest($"/issue/{input.IssueKey}", Method.Put, authenticationCredentialsProviders);
            request.AddJsonBody(new
            {
                fields = new
                {
                    priority = new
                    {
                        id = input.PriorityId
                    }
                }
            });
            await client.ExecuteAsync(request);
        }

        [Action("Delete issue", Description = "Delete an issue. To delete an issue with subtasks, set DeleteSubtasks")]
        public async Task DeleteIssue(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DeleteIssueRequest input)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var endpoint = QueryHelpers.AddQueryString($"/issue/{input.IssueKey}", 
                new Dictionary<string, string>() { { "deleteSubtasks", input.DeleteSubtasks } });
            var request = new JiraRequest(endpoint, Method.Delete, authenticationCredentialsProviders);
            await client.ExecuteAsync(request);
        }
    }
}
