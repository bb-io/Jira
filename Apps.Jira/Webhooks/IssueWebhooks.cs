using Apps.Jira.Webhooks.Handlers.IssueHandlers;
using Apps.Jira.Webhooks.Payload;
using Apps.Jira.Webhooks.Responses;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;
using System.Net;
using Apps.Jira.Dtos;
using Apps.Jira.Webhooks.Inputs;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.Webhooks
{
    [WebhookList]
    public class IssueWebhooks : BaseInvocable
    {
        private IEnumerable<AuthenticationCredentialsProvider> Creds =>
            InvocationContext.AuthenticationCredentialsProviders;
        
        public IssueWebhooks(InvocationContext invocationContext) : base(invocationContext)
        {
        }
        
        [Webhook("On issue updated", typeof(IssueUpdatedHandler), 
            Description = "This webhook is triggered when an issue is updated.")]
        public async Task<WebhookResponse<IssueResponse>> IssueUpdated(WebhookRequest request, 
            [WebhookParameter] IssueInput issue, [WebhookParameter] ProjectInput project)
        {
            var payload = DeserializePayload(request);

            if ((project.ProjectKey is not null && !project.ProjectKey.Equals(payload.Issue.Fields.Project.Key)) ||
                (issue.IssueKey is not null && !issue.IssueKey.Equals(payload.Issue.Key)))
                return new WebhookResponse<IssueResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };

            var issueResponse = CreateIssueResponse(payload);
            return issueResponse;
        }
        
        [Webhook("On issue created", typeof(IssueCreatedHandler), 
            Description = "This webhook is triggered when an issue is created.")]
        public async Task<WebhookResponse<IssueResponse>> IssueCreated(WebhookRequest request,
            [WebhookParameter] ProjectInput project)
        {
            var payload = DeserializePayload(request);
        
            if (project.ProjectKey is not null && !project.ProjectKey.Equals(payload.Issue.Fields.Project.Key))
                return new WebhookResponse<IssueResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };
        
            var issueResponse = CreateIssueResponse(payload);
            return issueResponse;
        }
        
        [Webhook("On issue assigned", typeof(IssueCreatedOrUpdatedHandler), 
            Description = "This webhook is triggered when an issue is assigned to specific user.")]
        public async Task<WebhookResponse<IssueResponse>> IssueAssigned(WebhookRequest request,
            [WebhookParameter] AssigneeInput assignee, [WebhookParameter] ProjectInput project)
        {
            var payload = DeserializePayload(request);
            var actualAssignee = payload.Changelog.Items.FirstOrDefault(item => item.FieldId == "assignee");

            if ((project.ProjectKey is not null && !project.ProjectKey.Equals(payload.Issue.Fields.Project.Key)) 
                || actualAssignee == null 
                || !assignee.AccountId.Equals(actualAssignee.To))
                return new WebhookResponse<IssueResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };
        
            var issueResponse = CreateIssueResponse(payload);
            return issueResponse;
        }
        
        [Webhook("On issue with specific type created", typeof(IssueCreatedOrUpdatedHandler), 
            Description = "This webhook is triggered when an issue created has specific type.")]
        public async Task<WebhookResponse<IssueResponse>> OnIssueWithSpecificTypeCreated(WebhookRequest request, 
            [WebhookParameter] IssueTypeInput issueType, [WebhookParameter] ProjectInput project)
        {
            var payload = DeserializePayload(request);
            
            if ((project.ProjectKey is not null && !project.ProjectKey.Equals(payload.Issue.Fields.Project.Key)) ||
                 !issueType.IssueType.Equals(payload.Issue.Fields.IssueType.Name))
                return new WebhookResponse<IssueResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };
        
            var issueResponse = CreateIssueResponse(payload);
            return issueResponse;
        }
        
        [Webhook("On issue with specific priority created", typeof(IssueCreatedOrUpdatedHandler), 
            Description = "This webhook is triggered when an issue created has specified priority or was updated to have specified priority.")]
        public async Task<WebhookResponse<IssueResponse>> IssueWithHighPriorityCreated(WebhookRequest request,
            [WebhookParameter] PriorityInput priority, [WebhookParameter] ProjectInput project)
        {
            var payload = DeserializePayload(request);
            
            if ((project.ProjectKey is not null && !project.ProjectKey.Equals(payload.Issue.Fields.Project.Key)) ||
                !payload.Changelog.Items.Any(item => item.FieldId == "priority") ||
                !priority.PriorityId.Equals(payload.Changelog.Items.First(item => item.FieldId == "priority").To))
                return new WebhookResponse<IssueResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };
        
            var issueResponse = CreateIssueResponse(payload);
            return issueResponse;
        }
        
        [Webhook("On issue deleted", typeof(IssueDeletedHandler), 
            Description = "This webhook is triggered when an issue is deleted.")]
        public async Task<WebhookResponse<IssueResponse>> IssueDeleted(WebhookRequest request,
            [WebhookParameter] ProjectInput project)
        {
            var payload = DeserializePayload(request);
        
            if (project.ProjectKey is not null && !project.ProjectKey.Equals(payload.Issue.Fields.Project.Key))
                return new WebhookResponse<IssueResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };
        
            var issueResponse = CreateIssueResponse(payload);
            return issueResponse;
        }
        
        [Webhook("On file attached to issue", typeof(IssueCreatedOrUpdatedHandler), 
            Description = "This webhook is triggered when a file is attached to an issue.")]
        public async Task<WebhookResponse<IssueAttachmentResponse>> OnFileAttachedToIssue(WebhookRequest request, 
            [WebhookParameter] IssueInput issue, [WebhookParameter] ProjectInput project)
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
        
            var jiraClient = new JiraClient(Creds);
            var getAttachmentRequest = new JiraRequest($"/attachment/{attachmentItem.To}", Method.Get, Creds);
            var attachment = await jiraClient.ExecuteWithHandling<AttachmentDto>(getAttachmentRequest);
        
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

        private WebhookPayload DeserializePayload(WebhookRequest request)
        {
            var payload = JsonConvert.DeserializeObject<WebhookPayload>(request.Body.ToString(), new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                }
            ) ?? throw new InvalidCastException(nameof(request.Body));
            return payload;
        }
        
        private WebhookResponse<IssueResponse> CreateIssueResponse(WebhookPayload payload)
        {
            var issue = payload.Issue;

            return new WebhookResponse<IssueResponse>
            {
                HttpResponseMessage = new HttpResponseMessage(statusCode: HttpStatusCode.OK),
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
                    Status = issue.Fields.Status.Name
                }
            };
        }
    }
}