using Apps.Jira.Webhooks.Handlers.IssueHandlers;
using Apps.Jira.Webhooks.Payload;
using Apps.Jira.Webhooks.Responses;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;
using System.Net;

namespace Apps.Jira.Webhooks
{
    //[WebhookList]
    public class IssueWebhooks
    {
        [Webhook("On issue updated", typeof(IssueUpdatedHandler), 
            Description = "This webhook is triggered when an issue is updated.")]
        public async Task<WebhookResponse<IssueResponse>> IssueUpdated(WebhookRequest request)
        {
            return HandleWebhookRequest(request);
        }
        
        [Webhook("On issue created", typeof(IssueCreatedInProjectHandler), 
            Description = "This webhook is triggered when an issue is created in specific project.")]
        public async Task<WebhookResponse<IssueResponse>> IssueCreatedInProject(WebhookRequest request)
        {
            return HandleWebhookRequest(request);
        }
        
        [Webhook("On issue assigned", typeof(IssueAssignedHandler), 
            Description = "This webhook is triggered when an issue is assigned to specific user.")]
        public async Task<WebhookResponse<IssueResponse>> IssueAssigned(WebhookRequest request)
        {
            return HandleWebhookRequest(request);
        }
        
        [Webhook("On bug issue created", typeof(BugIssueCreatedHandler), 
            Description = "This webhook is triggered when an issue created is a bug.")]
        public async Task<WebhookResponse<IssueResponse>> BugIssueCreated(WebhookRequest request)
        {
            return HandleWebhookRequest(request);
        }
        
        [Webhook("On issue with high priority created", typeof(IssueWithHighPriorityCreatedHandler), 
            Description = "This webhook is triggered when an issue created has a high or highest priority or was updated to a high or highest priority.")]
        public async Task<WebhookResponse<IssueResponse>> IssueWithHighPriorityCreated(WebhookRequest request)
        {
            return HandleWebhookRequest(request);
        }

        private WebhookResponse<IssueResponse> HandleWebhookRequest(WebhookRequest request)
        {
            var data = JsonConvert.DeserializeObject<IssueWrapper>(request.Body.ToString(), new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                }
            );
            return new WebhookResponse<IssueResponse>
            {
                HttpResponseMessage = new HttpResponseMessage(statusCode: HttpStatusCode.OK),
                Result = new IssueResponse
                {
                    IssueKey = data.Issue.Key,
                    ProjectKey = data.Issue.Fields.Project.Key,
                    Summary = data.Issue.Fields.Summary,
                    Description = data.Issue.Fields.Description ?? "No Description",
                    IssueType = data.Issue.Fields.IssueType.Name,
                    Priority = data.Issue.Fields.Priority?.Name ?? "No Priority",
                    AssigneeName = data.Issue.Fields.Assignee?.DisplayName ?? "Unassigned",
                    AssigneeAccountId = data.Issue.Fields.Assignee?.AccountId ?? "Unassigned",
                    Status = data.Issue.Fields.Status.Name
                }
            };
        }
    }
}