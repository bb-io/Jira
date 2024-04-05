using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using Newtonsoft.Json;
using RestSharp;
using Method = RestSharp.Method;

namespace Apps.Jira.Actions;

[ActionList]
public class IssueActions : JiraInvocable
{
    private readonly IFileManagementClient _fileManagementClient;

    public IssueActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
        : base(invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

    #region GET

    [Action("Get issue", Description = "Get the specified issue.")]
    public async Task<IssueDto> GetIssueByKey([ActionParameter] IssueIdentifier input)
    {
        var request = new JiraRequest($"/issue/{input.IssueKey}", Method.Get);
        var issue = await Client.ExecuteWithHandling<IssueWrapper>(request);
        return new IssueDto(issue);
    }

    [Action("List recently created issues", Description =
        "List issues created during past hours in a specific project." +
        "If number of hours is not specified, issues created during " +
        "past 24 hours are listed.")]
    public async Task<IssuesResponse> ListRecentlyCreatedIssues([ActionParameter] ProjectIdentifier project,
        [ActionParameter] [Display("Hours")] int? hours)
    {
        var request = new JiraRequest($"/search?jql=project={project.ProjectKey} and Created >-{hours ?? 24}h",
            Method.Get);
        var issues = await Client.ExecuteWithHandling<IssuesWrapper>(request);
        return new IssuesResponse
        {
            Issues = issues.Issues.Select(i => new IssueDto(i))
        };
    }

    [Action("List attachments", Description = "List files attached to an issue.")]
    public async Task<AttachmentsResponse> ListAttachments([ActionParameter] IssueIdentifier issue)
    {
        var request = new JiraRequest($"/issue/{issue.IssueKey}", Method.Get);
        var result = await Client.ExecuteWithHandling<IssueWrapper>(request);
        var attachments = result.Fields.Attachment ?? new AttachmentDto[] { };
        return new AttachmentsResponse { Attachments = attachments };
    }

    [Action("Download attachment", Description = "Download an attachment.")]
    public async Task<DownloadAttachmentResponse> DownloadAttachment([ActionParameter] AttachmentIdentifier attachment)
    {
        var request = new JiraRequest($"/attachment/content/{attachment.AttachmentId}", Method.Get);
        var response = await Client.ExecuteWithHandling(request);
        var filename = response.ContentHeaders.First(h => h.Name == "Content-Disposition").Value.ToString()
            .Split('"')[1];
        var contentType = response.ContentHeaders.First(h => h.Name == "Content-Type").Value.ToString();

        using var stream = new MemoryStream(response.RawBytes);
        var file = await _fileManagementClient.UploadAsync(stream, contentType, filename);
        return new DownloadAttachmentResponse { Attachment = file };
    }

    #endregion

    #region POST

    [Action("Create issue", Description = "Create a new issue.")]
    public async Task<CreatedIssueDto> CreateIssue([ActionParameter] ProjectIdentifier project,
        [ActionParameter] CreateIssueRequest input)
    {
        var request = new JiraRequest("/issue", Method.Post);

        var accountId = input.AccountId;

        if (int.TryParse(accountId, out var accountIntId) && accountIntId == int.MinValue)
            accountId = null;

        request.AddJsonBody(new
        {
            fields = new
            {
                assignee = new { id = accountId },
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
                                    text = input.Description ?? ""
                                }
                            }
                        }
                    }
                },
                issuetype = new { id = input.IssueTypeId }
            }
        });
        var createdIssue = await Client.ExecuteWithHandling<CreatedIssueDto>(request);
        return createdIssue;
    }

    [Action("Add attachment", Description = "Add attachment to an issue.")]
    public async Task<AttachmentDto> AddAttachment([ActionParameter] IssueIdentifier issue,
        [ActionParameter] AddAttachmentRequest input)
    {
        var request = new JiraRequest($"/issue/{issue.IssueKey}/attachments", Method.Post);
        var attachmentStream = await _fileManagementClient.DownloadAsync(input.Attachment);
        var attachmentBytes = await attachmentStream.GetByteData();
        request.AddHeader("X-Atlassian-Token", "no-check");
        request.AddFile("file", attachmentBytes, input.Attachment.Name);
        var response = await Client.ExecuteWithHandling<IEnumerable<AttachmentDto>>(request);
        return response.First();
    }

    #endregion

    #region PUT

    [Action("Update issue", Description = "Update issue, specifying only the fields that require updating.")]
    public async Task UpdateIssue([ActionParameter] ProjectIdentifier projectIdentifier,
        [ActionParameter] IssueIdentifier issue,
        [ActionParameter] UpdateIssueRequest input)
    {
        if (input.AssigneeAccountId != null)
        {
            var accountId = input.AssigneeAccountId;

            if (int.TryParse(accountId, out var accountIntId) && accountIntId == int.MinValue)
                accountId = null;

            var request = new JiraRequest($"/issue/{issue.IssueKey}/assignee", Method.Put)
                .WithJsonBody(new { accountId });

            await Client.ExecuteWithHandling(request);
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

            var request = new JiraRequest($"/issue/{issue.IssueKey}", Method.Put)
                .WithJsonBody(jsonBody,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            await Client.ExecuteWithHandling(request);
        }

        if (input.StatusId != null)
        {
            var getTransitionsRequest = new JiraRequest($"/issue/{issue.IssueKey}/transitions", Method.Get);
            var transitions = await Client.ExecuteWithHandling<TransitionsResponse>(getTransitionsRequest);

            var targetTransition = transitions.Transitions
                .FirstOrDefault(transition => transition.To.Id == input.StatusId);

            if (targetTransition != null)
            {
                var transitionRequest = new JiraRequest($"/issue/{issue.IssueKey}/transitions", Method.Post)
                    .WithJsonBody(new { transition = new { id = targetTransition.Id } });

                await Client.ExecuteWithHandling(transitionRequest);
            }
        }
    }

    #endregion

    #region DELETE

    [Action("Delete issue", Description = "Delete an issue. To delete the issue along with its subtasks, " +
                                          "set the optional input parameter 'Delete subtasks' to 'True'.")]
    public async Task DeleteIssue([ActionParameter] IssueIdentifier issue,
        [ActionParameter] [Display("Delete subtasks")]
        bool? deleteSubtasks)
    {
        var request =
            new JiraRequest($"/issue/{issue.IssueKey}?deleteSubtasks={(deleteSubtasks ?? false).ToString()}",
                Method.Delete);

        await Client.ExecuteWithHandling(request);
    }

    #endregion
}