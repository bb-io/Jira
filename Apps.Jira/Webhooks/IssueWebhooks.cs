using Apps.Jira.Webhooks.Handlers.IssueHandlers;
using Apps.Jira.Webhooks.Payload;
using Apps.Jira.Webhooks.Responses;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;
using System.Net;
using Apps.Jira.Dtos;
using Apps.Jira.Webhooks.Inputs;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;

namespace Apps.Jira.Webhooks
{
    [WebhookList]
    public class IssueWebhooks(InvocationContext invocationContext) : JiraInvocable(invocationContext)
    {
        private IEnumerable<AuthenticationCredentialsProvider> Creds =>
            InvocationContext.AuthenticationCredentialsProviders;

        [Webhook("On issue updated", typeof(IssueUpdatedHandler), 
            Description = "This webhook is triggered when an issue is updated.")]
        public async Task<WebhookResponse<IssueResponse>> OnIssueUpdated(WebhookRequest request, 
            [WebhookParameter] IssueInput issue, 
            [WebhookParameter] ProjectIssueInput project,
            [WebhookParameter] LabelsOptionalInput labels)
        {
            var payload = DeserializePayload(request);
            
            if ((project.ProjectKey is not null && !project.ProjectKey.Contains(payload.Issue.Fields.Project.Key)) ||
                (issue.IssueKey is not null && !issue.IssueKey.Equals(payload.Issue.Key)))
                return new WebhookResponse<IssueResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };

            var issueResponse = CreateIssueResponse(payload, labels);
            return issueResponse;
        }
        
        [Webhook("On issue created", typeof(IssueCreatedHandler), 
            Description = "This webhook is triggered when an issue is created.")]
        public async Task<WebhookResponse<IssueResponse>> OnIssueCreated(WebhookRequest request,
            [WebhookParameter] ProjectIssueInput project,
            [WebhookParameter] LabelsOptionalInput labels)
        {
            var payload = DeserializePayload(request);
        
            if (project.ProjectKey is not null && !project.ProjectKey.Contains(payload.Issue.Fields.Project.Key))
                return new WebhookResponse<IssueResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };
        
            var issueResponse = CreateIssueResponse(payload, labels);
            return issueResponse;
        }
        
        [Webhook("On issue assigned", typeof(IssueCreatedOrUpdatedHandler), 
            Description = "This webhook is triggered when an issue is assigned to specific user.")]
        public async Task<WebhookResponse<IssueResponse>> IssueAssigned(WebhookRequest request,
            [WebhookParameter] AssigneeInput assignee, 
            [WebhookParameter] ProjectInput project,
            [WebhookParameter] LabelsOptionalInput labels)
        {
            var payload = DeserializePayload(request);
            var actualAssignee = payload.Changelog.Items.FirstOrDefault(item => item.FieldId == "assignee");

            if ((project.ProjectKey is not null && !project.ProjectKey.Equals(payload.Issue.Fields.Project.Key)) 
                || actualAssignee is null)
                return new WebhookResponse<IssueResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };

            if (assignee.AccountId == "-1")
            {
                var getProjectRequest = new JiraRequest($"/project/{payload.Issue.Fields.Project.Key}", Method.Get);
                var projectDto = await Client.ExecuteWithHandling<DetailedProjectDto>(getProjectRequest);
                if ((projectDto.DefaultAssignee == "UNASSIGNED" && actualAssignee.To is not null) 
                    || (projectDto.DefaultAssignee == "PROJECT_LEAD" && actualAssignee.To != projectDto.Lead.AccountId)) 
                    return new WebhookResponse<IssueResponse>
                    {
                        HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                        ReceivedWebhookRequestType = WebhookRequestType.Preflight
                    };
                
            }
            else
            {
                if (!assignee.AccountId.Equals(actualAssignee.To))
                    return new WebhookResponse<IssueResponse>
                    {
                        HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                        ReceivedWebhookRequestType = WebhookRequestType.Preflight
                    };
            }
        
            var issueResponse = CreateIssueResponse(payload, labels);
            return issueResponse;
        }
        
        [Webhook("On issue with specific type created", typeof(IssueCreatedOrUpdatedHandler), 
            Description = "This webhook is triggered when an issue created has specific type or issue was updated to have specific type.")]
        public async Task<WebhookResponse<IssueResponse>> OnIssueWithSpecificTypeCreated(WebhookRequest request, 
            [WebhookParameter] IssueTypeInput issueType, 
            [WebhookParameter] ProjectInput project,
            [WebhookParameter] LabelsOptionalInput labels)
        {
            var payload = DeserializePayload(request);
            var issueTypeItem = payload.Changelog.Items.FirstOrDefault(item => item.FieldId == "issuetype");
            
            if ((issueTypeItem is null && payload.WebhookEvent == "jira:issue_updated")
                || (project.ProjectKey is not null && !project.ProjectKey.Equals(payload.Issue.Fields.Project.Key)) 
                || !issueType.IssueType.Equals(payload.Issue.Fields.IssueType.Name))
                return new WebhookResponse<IssueResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };
        
            var issueResponse = CreateIssueResponse(payload, labels);
            return issueResponse;
        }
        
        [Webhook("On issue with specific priority created", typeof(IssueCreatedOrUpdatedHandler), 
            Description = "This webhook is triggered when an issue created has specified priority or issue was updated to have specified priority.")]
        public async Task<WebhookResponse<IssueResponse>> OnIssueWithSpecificPriorityCreated(WebhookRequest request,
            [WebhookParameter] PriorityInput priority, 
            [WebhookParameter] ProjectInput project,
            [WebhookParameter] LabelsOptionalInput labels)
        {
            var payload = DeserializePayload(request);
            var priorityItem = payload.Changelog.Items.FirstOrDefault(item => item.FieldId == "priority");
            
            if (priorityItem == null 
                || (project.ProjectKey is not null && !project.ProjectKey.Equals(payload.Issue.Fields.Project.Key))
                || !priority.PriorityId.Equals(priorityItem.To))
                return new WebhookResponse<IssueResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };
        
            var issueResponse = CreateIssueResponse(payload, labels);
            return issueResponse;
        }
        
        [Webhook("On issue deleted", typeof(IssueDeletedHandler), 
            Description = "This webhook is triggered when an issue is deleted.")]
        public async Task<WebhookResponse<IssueResponse>> OnIssueDeleted(WebhookRequest request,
            [WebhookParameter] ProjectInput project,
            [WebhookParameter] LabelsOptionalInput labels)
        {
            var payload = DeserializePayload(request);
        
            if (project.ProjectKey is not null && !project.ProjectKey.Equals(payload.Issue.Fields.Project.Key))
                return new WebhookResponse<IssueResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };
        
            var issueResponse = CreateIssueResponse(payload, labels);
            return issueResponse;
        }
        
        [Webhook("On file attached to issue", typeof(IssueUpdatedHandler), 
            Description = "This webhook is triggered when a file is attached to an issue.")]
        public async Task<WebhookResponse<IssueAttachmentResponse>> OnFileAttachedToIssue(WebhookRequest request, 
            [WebhookParameter] IssueInput issue, 
            [WebhookParameter] ProjectInput project)
        {
            var payload = DeserializePayload(request);
            var attachmentItem = payload.Changelog.Items.FirstOrDefault(item => item.FieldId == "attachment");
            
            if (attachmentItem is null 
                || (project.ProjectKey is not null && !project.ProjectKey.Equals(payload.Issue.Fields.Project.Key)) 
                || (issue.IssueKey is not null && !issue.IssueKey.Equals(payload.Issue.Key)))
                return new WebhookResponse<IssueAttachmentResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };
            
            var attachment = payload.Issue.Fields.Attachment.First(a => a.Id == attachmentItem.To);
            return new WebhookResponse<IssueAttachmentResponse>
            {
                HttpResponseMessage = new HttpResponseMessage(statusCode: HttpStatusCode.OK),
                Result = new IssueAttachmentResponse
                {
                    IssueKey = payload.Issue.Key,
                    ProjectKey = payload.Issue.Fields.Project.Key,
                    Attachment = attachment
                }
            };
        }
        
        [Webhook("On issue status changed", typeof(IssueUpdatedHandler), 
            Description = "This webhook is triggered when issue status is changed.")]
        public async Task<WebhookResponse<IssueResponse>> OnIssueStatusChanged(WebhookRequest request,
            [WebhookParameter] ProjectIdentifier project, 
            [WebhookParameter] OptionalStatusInput status, 
            [WebhookParameter] IssueInput issue,
            [WebhookParameter] LabelsOptionalInput labels)
        {
            var payload = DeserializePayload(request);
            var statusItem = payload.Changelog.Items.FirstOrDefault(item => item.FieldId == "status");

            if (statusItem is null 
                || (project.ProjectKey is not null && !project.ProjectKey.Equals(payload.Issue.Fields.Project.Key))
                || (status.StatusId is not null && payload.Issue.Fields.Status.Id != status.StatusId)
                || (issue.IssueKey is not null && !issue.IssueKey.Equals(payload.Issue.Key)))
                return new WebhookResponse<IssueResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };

            var issueResponse = CreateIssueResponse(payload, labels);
            return issueResponse;
        }

        private WebhookPayload DeserializePayload(WebhookRequest request)
        {
            var payload = JsonConvert.DeserializeObject<WebhookPayload>(request.Body.ToString(), new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                }
            ) ?? throw new InvalidCastException(nameof(request.Body));
            return payload;
        }
        
        private WebhookResponse<IssueResponse> CreateIssueResponse(WebhookPayload payload, LabelsOptionalInput labelsInput)
        {
            var issue = payload.Issue;
            
            if (labelsInput.Labels is not null && labelsInput.Labels.Any())
            {
                var labelsMatch = labelsInput.Labels.All(label => issue.Fields.Labels.Contains(label));
                if (!labelsMatch)
                {
                    return new WebhookResponse<IssueResponse>
                    {
                        HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                        ReceivedWebhookRequestType = WebhookRequestType.Preflight
                    };
                }
            }
            
            if(labelsInput.LabelsDropDown is not null && labelsInput.LabelsDropDown.Any())
            {
                var labelsMatch = labelsInput.LabelsDropDown.All(label => issue.Fields.Labels.Contains(label));
                if (!labelsMatch)
                {
                    return new WebhookResponse<IssueResponse>
                    {
                        HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                        ReceivedWebhookRequestType = WebhookRequestType.Preflight
                    };
                }
            }

            return new WebhookResponse<IssueResponse>
            {
                HttpResponseMessage = new HttpResponseMessage(statusCode: HttpStatusCode.OK),
                ReceivedWebhookRequestType = WebhookRequestType.Default,
                Result = new IssueResponse
                {
                    IssueKey = issue.Key,
                    ProjectKey = issue.Fields.Project.Key,
                    Summary = issue.Fields.Summary,
                    Description = issue.Fields.Description,
                    IssueType = issue.Fields.IssueType.Name,
                    Priority = issue.Fields.Priority?.Name,
                    AssigneeName = issue.Fields.Assignee?.DisplayName,
                    AssigneeAccountId = issue.Fields.Assignee?.AccountId,
                    Status = issue.Fields.Status.Name,
                    Attachments = issue.Fields.Attachment,
                    Labels = issue.Fields.Labels
                }
            };
        }
    }
}