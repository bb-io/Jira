using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using RestSharp;
using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.AspNetCore.WebUtilities;

namespace Apps.Jira
{
    [ActionList]
    public class Actions : BaseInvocable
    {
        private IEnumerable<AuthenticationCredentialsProvider> Creds =>
            InvocationContext.AuthenticationCredentialsProviders;

        public Actions(InvocationContext invocationContext) : base(invocationContext) { }
        
        [Action("Get issue", Description = "Get the specified issue.")]
        public async Task<IssueResponse> GetIssueByKey(
            [ActionParameter] IssueRequest input)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{input.IssueKey}", Method.Get, Creds);
            var issue = await client.ExecuteWithHandling<IssueWrapper>(request);
            return new IssueResponse
            {
                IssueKey = issue.Key,
                Summary = issue.Fields.Summary,
                Status = issue.Fields.Status.Name,
                Priority = issue.Fields.Priority.Name,
                Assignee = issue.Fields.Assignee,
                Project = issue.Fields.Project,
                Description = issue.Fields.Description == null ? ""
                    : string.Join('\n',
                        issue.Fields.Description.Content.Select(x => string.Join('\n', x.Content.Select(c => c.Text)))
                            .ToArray())
            };
        }

        [Action("Issue transition", Description = "Perform issue transition.")]
        public async Task IssueTransition(
            [ActionParameter] IssueTransitionRequest input)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{input.IssueKey}/transitions", Method.Post, Creds);
            request.AddJsonBody(new
            {
                transition = new
                {
                    id = input.TransitionId
                }
            });
            await client.ExecuteWithHandling(request);
        }

        [Action("Get issue transitions", Description = "Get a list of available transitions for specific issue.")]
        public async Task<TransitionsResponse> GetIssueTransitions(
            [ActionParameter] GetIssueTransitionsRequest input)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{input.IssueKey}/transitions", Method.Get, Creds);
            var transitions = await client.ExecuteWithHandling<TransitionsResponse>(request);
            return transitions;
        }

        [Action("Get all users", Description = "Get a list of users.")]
        public async Task<UsersResponse> GetAllUsers()
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest("/users/search", Method.Get, Creds);
            var users = await client.ExecuteWithHandling<UsersResponse>(request);
            return users;
        }

        [Action("Create issue", Description = "Create a new issue.")]
        public async Task CreateIssue([ActionParameter] CreateIssueRequest input)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest("/issue", Method.Post, Creds);
            request.AddJsonBody(new
            {
                fields = new
                {
                    assignee = new { id = input.AssigneeId },
                    project = new { key = input.ProjectKey },
                    summary = input.Summary,
                    description = new
                    {
                        Version = 1,
                        Type = "doc",
                        Content = new[]
                        {
                            new
                            {
                                Type = "paragraph", 
                                Content = new[]
                                {
                                    new
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
            await client.ExecuteWithHandling(request);
        }

        [Action("Assign issue", Description = "Assign an issue to a user. Is AccountId is set to -1, issue is assigned " +
                                              "to the default assignee for the project. Is AccountId is set to null, " +
                                              "the issue is set to unassigned.")]
        public async Task AssignIssue([ActionParameter] AssignIssueRequest input)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{input.IssueKey}/assignee", Method.Put, Creds);
            request.AddJsonBody(new { accountId = input.AccountId });
            await client.ExecuteWithHandling(request);
        }

        [Action("Update issue summary", Description = "Update summary for an issue.")]
        public async Task UpdateIssueSummary([ActionParameter] UpdateIssueSummaryRequest input)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{input.IssueKey}", Method.Put, Creds);
            request.AddJsonBody(new
            {
                fields = new { summary = input.Summary }
            });
            await client.ExecuteWithHandling(request);
        }

        [Action("Update issue description", Description = "Update description for an issue.")]
        public async Task UpdateIssueDescription([ActionParameter] UpdateDescriptionRequest input)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{input.IssueKey}", Method.Put, Creds);
            request.AddJsonBody(new
            {
                fields = new
                {
                    description = new
                    {
                        Version = 1,
                        Type = "doc",
                        Content = new[]
                        {
                            new
                            {
                                Type = "paragraph", 
                                Content = new[]
                                {
                                    new
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
            await client.ExecuteWithHandling(request);
        }

        [Action("Prioritize issue", Description = "Set priority for an issue.")]
        public async Task PrioritizeIssue([ActionParameter] PrioritizeIssueRequest input)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{input.IssueKey}", Method.Put, Creds);
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
            await client.ExecuteWithHandling(request);
        }

        [Action("Delete issue", Description = "Delete an issue. To delete an issue with subtasks, set DeleteSubtasks.")]
        public async Task DeleteIssue([ActionParameter] DeleteIssueRequest input)
        {
            var client = new JiraClient(Creds);
            var endpoint = QueryHelpers.AddQueryString($"/issue/{input.IssueKey}", 
                new Dictionary<string, string> { { "deleteSubtasks", input.DeleteSubtasks } });
            var request = new JiraRequest(endpoint, Method.Delete, Creds);
            await client.ExecuteWithHandling(request);
        }
    }
}
