using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using RestSharp;
using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.AspNetCore.WebUtilities;
using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Jira
{
    [ActionList]
    public class Actions : BaseInvocable
    {
        private IEnumerable<AuthenticationCredentialsProvider> Creds =>
            InvocationContext.AuthenticationCredentialsProviders;

        public Actions(InvocationContext invocationContext) : base(invocationContext) { }
        
        #region GET
        
        [Action("Get issue", Description = "Get the specified issue.")]
        public async Task<IssueDto> GetIssueByKey([ActionParameter] IssueIdentifier input)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{input.IssueKey}", Method.Get, Creds);
            var issue = await client.ExecuteWithHandling<IssueWrapper>(request);
            return new IssueDto(issue);
        }
        
        [Action("List recently created issues", Description = "List issues created during past hours in a specific project." +
                                                              "If number of hours is not specified, issues created during " +
                                                              "past 24 hours are listed.")]
        public async Task<IssuesResponse> ListRecentlyCreatedIssues([ActionParameter] ProjectIdentifier project,
            [ActionParameter] [Display("Hours")] int? hours)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/search?jql=project={project.ProjectKey} and Created >-{hours ?? 24}h", 
                Method.Get, Creds);
            var issues = await client.ExecuteWithHandling<IssuesWrapper>(request);
            return new IssuesResponse
            {
                Issues = issues.Issues.Select(i => new IssueDto(i))
            };
        }
        
        [Action("Get issue transitions", Description = "Get a list of available transitions for specific issue.")]
        public async Task<TransitionsResponse> GetIssueTransitions([ActionParameter] IssueIdentifier issue)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{issue.IssueKey}/transitions", Method.Get, Creds);
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
        
        [Action("List attachments", Description = "List files attached to an issue.")]
        public async Task<AttachmentsResponse> ListAttachments([ActionParameter] IssueIdentifier issue)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{issue.IssueKey}", Method.Get, Creds);
            var result = await client.ExecuteWithHandling<IssueWrapper>(request);
            var attachments = result.Fields.Attachment ?? new AttachmentDto[] { };
            return new AttachmentsResponse { Attachments = attachments };
        }
        
        [Action("Download attachment", Description = "Download an attachment.")]
        public async Task<DownloadAttachmentResponse> DownloadAttachment([ActionParameter] AttachmentIdentifier attachment)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/attachment/content/{attachment.AttachmentId}", Method.Get, Creds);
            var response = await client.ExecuteWithHandling(request);
            var fileBytes = response.RawBytes;
            var filenameHeader = response.ContentHeaders.First(h => h.Name == "Content-Disposition");
            var filename = filenameHeader.Value.ToString().Split('"')[1];
            var contentType = response.ContentHeaders.First(h => h.Name == "Content-Type").Value.ToString();
            
            return new DownloadAttachmentResponse
            {
                Attachment = new File(fileBytes)
                {
                    Name = filename,
                    ContentType = contentType
                } 
            };
        }

        #endregion
        
        #region POST
        
        [Action("Transition issue", Description = "Perform issue transition.")]
        public async Task TransitionIssue([ActionParameter] IssueIdentifier issue, 
            [ActionParameter] TransitionIdentifier transition)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{issue.IssueKey}/transitions", Method.Post, Creds);
            request.AddJsonBody(new
            {
                transition = new
                {
                    id = transition.TransitionId
                }
            });
            await client.ExecuteWithHandling(request);
        }
        
        [Action("Create issue", Description = "Create a new issue.")]
        public async Task<CreatedIssueDto> CreateIssue([ActionParameter] AssigneeIdentifier assignee, 
            [ActionParameter] ProjectIdentifier project, [ActionParameter] CreateIssueRequest input)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest("/issue", Method.Post, Creds);
            request.AddJsonBody(new
            {
                fields = new
                {
                    assignee = new { id = assignee.AccountId },
                    project = new { key = project.ProjectKey },
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
                                        Text = input.Description ?? ""
                                    }
                                }
                            }
                        }
                    },
                    issuetype = new { name = input.IssueType }
                }
            });
            var createdIssue = await client.ExecuteWithHandling<CreatedIssueDto>(request);
            return createdIssue;
        }
        
        [Action("Add attachment", Description = "Add attachment to an issue.")]
        public async Task<AttachmentDto> AddAttachment([ActionParameter] IssueIdentifier issue,
            [ActionParameter] AddAttachmentRequest input)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{issue.IssueKey}/attachments", Method.Post, Creds);
            request.AddHeader("X-Atlassian-Token", "no-check");
            request.AddFile("file", input.Attachment.Bytes, input.Attachment.Name);
            var response = await client.ExecuteWithHandling<IEnumerable<AttachmentDto>>(request);
            return response.First();
        }

        #endregion

        #region PUT
        
        [Action("Assign issue", Description = "Assign an issue to a user. If assignee is not specified, the issue is " +
                                               "set to unassigned.")]
        public async Task AssignIssue([ActionParameter] IssueIdentifier issue, 
            [ActionParameter] AssigneeIdentifier assignee)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{issue.IssueKey}/assignee", Method.Put, Creds);
            request.AddJsonBody(new { accountId = assignee.AccountId });
            await client.ExecuteWithHandling(request);
        }

        [Action("Update issue summary", Description = "Update summary for an issue.")]
        public async Task UpdateIssueSummary([ActionParameter] IssueIdentifier issue, 
            [ActionParameter] [Display("Summary")] string summary)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{issue.IssueKey}", Method.Put, Creds);
            request.AddJsonBody(new
            {
                fields = new { summary }
            });
            await client.ExecuteWithHandling(request);
        }

        [Action("Update issue description", Description = "Update description for an issue.")]
        public async Task UpdateIssueDescription([ActionParameter] IssueIdentifier issue, 
            [ActionParameter] [Display("Description")] string description)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{issue.IssueKey}", Method.Put, Creds);
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
                                        Text = description
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
        public async Task PrioritizeIssue([ActionParameter] IssueIdentifier issue, 
            [ActionParameter] PriorityIdentifier priority)
        {
            var client = new JiraClient(Creds);
            var request = new JiraRequest($"/issue/{issue.IssueKey}", Method.Put, Creds);
            request.AddJsonBody(new
            {
                fields = new
                {
                    priority = new
                    {
                        id = priority.PriorityId
                    }
                }
            });
            await client.ExecuteWithHandling(request);
        }
        
        #endregion
        
        #region DELETE
        
        [Action("Delete issue", Description = "Delete an issue. To delete an issue with subtasks, set DeleteSubtasks.")]
        public async Task DeleteIssue([ActionParameter] IssueIdentifier issue, 
            [ActionParameter] [Display("Delete subtasks")] bool deleteSubtasks)
        {
            var client = new JiraClient(Creds);
            var endpoint = QueryHelpers.AddQueryString($"/issue/{issue.IssueKey}", 
                new Dictionary<string, string> { { "deleteSubtasks", deleteSubtasks.ToString() } });
            var request = new JiraRequest(endpoint, Method.Delete, Creds);
            await client.ExecuteWithHandling(request);
        }
        
        #endregion
    }
}
