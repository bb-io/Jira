﻿using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using Apps.Jira.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using Newtonsoft.Json;
using RestSharp;
using Method = RestSharp.Method;
using Apps.Jira.Webhooks.Payload;

namespace Apps.Jira.Actions;

[ActionList]
public class IssueActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : JiraInvocable(invocationContext)
{
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
        var file = await fileManagementClient.UploadAsync(stream, contentType, filename);
        return new DownloadAttachmentResponse { Attachment = file };
    }

    [Action("Get issue type details", Description = "Get issue type details by name")]
    public async Task<IssueTypeDto> GetIssueTypeDetails([ActionParameter] ProjectIdentifier projectIdentifier, [Display("Type name")][ActionParameter] string TypeName)
    {
        var getProjectRequest = new JiraRequest($"/project/{projectIdentifier.ProjectKey}", Method.Get);
        var project = await Client.ExecuteWithHandling<ProjectDto>(getProjectRequest);

        var getIssueTypesRequest = new JiraRequest("/issuetype", Method.Get);
        var issueTypes = await Client.ExecuteWithHandling<IEnumerable<IssueTypeDto>>(getIssueTypesRequest);
        try
        {
            return issueTypes.Where(type => type.Scope is null || type.Scope?.Type == "PROJECT" && type.Scope.Project!.Id == project.Id)
            .First(x => x.Name.ToLower() == TypeName.ToLower());
        }
        catch
        {
            return null;
        }
    }
    #endregion

    #region POST

    [Action("Create issue", Description = "Create a new issue.")]
    public async Task<CreatedIssueDto> CreateIssue([ActionParameter] ProjectIdentifier project,
        [ActionParameter] CreateIssueRequest input)
    {
        var fields = new Dictionary<string, object>
        {
            { "project", new { key = project.ProjectKey } },
            { "summary", input.Summary },
            { "description", new
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
                }
            },
            { "issuetype", new { id = input.IssueTypeId } }
        };

        var accountId = input.AccountId;
        if (!string.IsNullOrEmpty(accountId))
        {
            fields.Add("assignee", new { id = accountId });
        }

        var request = new JiraRequest("/issue", Method.Post).AddJsonBody(new
        {
            fields = fields
        });

        var createdIssue = await Client.ExecuteWithHandling<CreatedIssueDto>(request);
        return createdIssue;
    }

    [Action("Add attachment", Description = "Add attachment to an issue.")]
    public async Task<AttachmentDto> AddAttachment([ActionParameter] IssueIdentifier issue,
        [ActionParameter] AddAttachmentRequest input)
    {
        var request = new JiraRequest($"/issue/{issue.IssueKey}/attachments", Method.Post);
        var attachmentStream = await fileManagementClient.DownloadAsync(input.Attachment);
        var attachmentBytes = await attachmentStream.GetByteData();
        request.AddHeader("X-Atlassian-Token", "no-check");
        request.AddFile("file", attachmentBytes, input.Attachment.Name);
        var response = await Client.ExecuteWithHandling<IEnumerable<AttachmentDto>>(request);
        return response.First();
    }

    [Action("Add labels to issue", Description = "Add labels to a specific issue.")]
    public async Task<IssueDto> AddLabelsToIssue([ActionParameter] IssueIdentifier issue,
        [ActionParameter] AddLabelsRequest input)
    {
        var request = new JiraRequest($"/issue/{issue.IssueKey}", Method.Put)
            .WithJsonBody(new { update = new { labels = input.Labels.Select(label => new { add = label }) } });
        await Client.ExecuteWithHandling(request);
        
        return await GetIssueByKey(issue);
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
            var descriptionJson = input.Description != null
                ? MarkdownToJiraConverter.ConvertMarkdownToJiraDoc(input.Description)
                : null;
            
            var jsonBody = new
            {
                fields = new
                {
                    summary = input.Summary,
                    description = descriptionJson,
                    issuetype = input.IssueTypeId != null ? new { id = input.IssueTypeId } : null
                }
            };
            
            var endpoint = $"/issue/{issue.IssueKey}";
            if (input.OverrideScreenSecurity.HasValue)
            {
                endpoint += $"?overrideScreenSecurity={input.OverrideScreenSecurity.Value}";
            }
            
            if (input.NotifyUsers.HasValue)
            {
                endpoint = endpoint + (input.OverrideScreenSecurity.HasValue ? "&" : "?") + $"notifyUsers={input.NotifyUsers}";
            }
            
            var request = new JiraRequest(endpoint, Method.Put)
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