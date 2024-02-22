using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using RestSharp;
using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.Jira
{
    [ActionList]
    public class Actions : BaseInvocable
    {
        private readonly IFileManagementClient _fileManagementClient;
        
        private IEnumerable<AuthenticationCredentialsProvider> Creds =>
            InvocationContext.AuthenticationCredentialsProviders;

        public Actions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) 
            : base(invocationContext)
        {
            _fileManagementClient = fileManagementClient;
        }
        
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
            var filename = response.ContentHeaders.First(h => h.Name == "Content-Disposition").Value.ToString()
                .Split('"')[1];
            var contentType = response.ContentHeaders.First(h => h.Name == "Content-Type").Value.ToString();

            using var stream = new MemoryStream(response.RawBytes);
            var file = await _fileManagementClient.UploadAsync(stream, contentType, filename);
            return new DownloadAttachmentResponse { Attachment = file };
        }

        [Action("Get custom string or dropdown field value",
            Description = "Get value of custom string or dropdown field of specific issue.")]
        public async Task<GetCustomFieldValueResponse<string>> GetCustomStringFieldValue(
            [ActionParameter] IssueIdentifier issue, [ActionParameter] CustomStringFieldIdentifier customStringField)
        {
            var client = new JiraClient(Creds);
            var targetField = await GetCustomFieldData(customStringField.CustomStringFieldId);
            var getIssueRequest = new JiraRequest($"/issue/{issue.IssueKey}", Method.Get, Creds);
            var getIssueResponse = await client.ExecuteWithHandling(getIssueRequest);
            var requestedField =
                JObject.Parse(getIssueResponse.Content)["fields"][customStringField.CustomStringFieldId];

            string requestedFieldValue;

            if (targetField.Schema!.Type == "string")
                requestedFieldValue = requestedField.ToString();
            else
                requestedFieldValue = requestedField["value"].ToString();
            
            return new GetCustomFieldValueResponse<string> { Value = requestedFieldValue };
        }

        #endregion
        
        #region POST
        
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
                        version = 1,
                        type = "doc",
                        content = new[]
                        {
                            new
                            {
                                type = "paragraph", 
                                content = new[]
                                {
                                    new
                                    {
                                        type = "text",
                                        xext = input.Description ?? ""
                                    }
                                }
                            }
                        }
                    },
                    issuetype = new { id = input.IssueTypeId }
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
            var attachmentStream = await _fileManagementClient.DownloadAsync(input.Attachment);
            var attachmentBytes = await attachmentStream.GetByteData();
            request.AddHeader("X-Atlassian-Token", "no-check");
            request.AddFile("file", attachmentBytes, input.Attachment.Name);
            var response = await client.ExecuteWithHandling<IEnumerable<AttachmentDto>>(request);
            return response.First();
        }

        #endregion

        #region PUT

        [Action("Update issue", Description = "Update issue, specifying only the fields that require updating.")]
        public async Task UpdateIssue([ActionParameter] ProjectIdentifier projectIdentifier,
            [ActionParameter] IssueIdentifier issue,
            [ActionParameter] UpdateIssueRequest input)
        {
            var client = new JiraClient(Creds);
            
            if (input.AssigneeAccountId != null)
            {
                var accountId = input.AssigneeAccountId;

                if (int.TryParse(accountId, out var accountIntId) && accountIntId == int.MinValue)
                    accountId = null;

                var request = new JiraRequest($"/issue/{issue.IssueKey}/assignee", Method.Put, Creds)
                    .WithJsonBody(new { accountId });
                
                await client.ExecuteWithHandling(request);
            }

            if (input.Summary != null || input.Description != null || input.IssueTypeId != null)
            {
                var jsonBody = new
                {
                    fields = new
                    {
                        summary = input.Summary,
                        description = input.Description != null
                            ? new
                            {
                                version = 1,
                                type = "doc",
                                content = new[]
                                {
                                    new
                                    {
                                        type = "paragraph",
                                        content = new[]
                                        {
                                            new
                                            {
                                                type = "text",
                                                text = input.Description
                                            }
                                        }
                                    }
                                }
                            }
                            : null,
                        issuetype = input.IssueTypeId != null ? new { id = input.IssueTypeId } : null
                    }
                };

                var request = new JiraRequest($"/issue/{issue.IssueKey}", Method.Put, Creds)
                    .WithJsonBody(jsonBody,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                await client.ExecuteWithHandling(request);
            }

            if (input.StatusId != null)
            {
                var getTransitionsRequest = new JiraRequest($"/issue/{issue.IssueKey}/transitions", Method.Get, 
                    Creds);
                var transitions = await client.ExecuteWithHandling<TransitionsResponse>(getTransitionsRequest);

                var targetTransition = transitions.Transitions
                    .FirstOrDefault(transition => transition.To.Id == input.StatusId);

                if (targetTransition != null)
                {
                    var transitionRequest = new JiraRequest($"/issue/{issue.IssueKey}/transitions", Method.Post, Creds)
                        .WithJsonBody(new { transition = new { id = targetTransition.Id } });
                
                    await client.ExecuteWithHandling(transitionRequest);
                }
            }
        }
        
        [Action("Set custom string or dropdown field value", 
            Description = "Set value of custom string or dropdown field of specific issue.")]
        public async Task SetCustomStringFieldValue([ActionParameter] IssueIdentifier issue, 
            [ActionParameter] CustomStringFieldIdentifier customStringField,
            [ActionParameter] [Display("Value")] string value)
        {
            var client = new JiraClient(Creds);
            var targetField = await GetCustomFieldData(customStringField.CustomStringFieldId);
            string requestBody;
            
            if (targetField.Schema!.Type == "string")
                requestBody = $@"
                {{
                    ""fields"": {{
                        ""{customStringField.CustomStringFieldId}"": ""{value}""
                    }}
                }}";
            else
                requestBody = $@"
                {{
                    ""fields"": {{
                        ""{customStringField.CustomStringFieldId}"": {{
                            ""value"": ""{value}""
                        }}
                    }}
                }}";
            
            var updateFieldRequest = new JiraRequest($"/issue/{issue.IssueKey}", Method.Put, Creds);
            updateFieldRequest.AddJsonBody(requestBody);
            
            try
            {
                await client.ExecuteWithHandling(updateFieldRequest);
            }
            catch
            {
                throw new Exception("Couldn't set field value. Please make sure that field exists for specific issue " +
                                    "type in the project.");
            }
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

        #region Utils

        public async Task<FieldDto> GetCustomFieldData(string customFieldId)
        {
            var client = new JiraClient(Creds);
            var getFieldsRequest = new JiraRequest("/field", Method.Get, Creds);
            var fields = await client.ExecuteWithHandling<IEnumerable<FieldDto>>(getFieldsRequest);
            return fields.First(field => field.Id == customFieldId);
        }

        #endregion
    }
}
