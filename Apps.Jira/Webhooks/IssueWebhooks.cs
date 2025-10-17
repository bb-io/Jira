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
using System.Net.Mail;
using Blackbird.Applications.Sdk.Common.Exceptions;

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
            [WebhookParameter] ProjectInput project,
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

            if (project.Field is { } fieldsToCheck && fieldsToCheck.Any())
            {
                var changedFields = payload.Changelog?.Items?
                    .Select(i => i.Field)
                    .Distinct()
                    .ToList() ?? new List<string>();

                bool isRelevantChange = changedFields
                    .Any(changed => fieldsToCheck.Contains(changed, StringComparer.OrdinalIgnoreCase));

                if (!isRelevantChange)
                {
                    return new WebhookResponse<IssueResponse>
                    {
                        HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                        ReceivedWebhookRequestType = WebhookRequestType.Preflight
                    };
                }
            }

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
            [WebhookParameter] ProjectIssueInput project,
            [WebhookParameter] LabelsOptionalInput labels)
        {
            var payload = DeserializePayload(request);
            var actualAssignee = payload.Changelog.Items.FirstOrDefault(item => item.FieldId == "assignee");

            if ((project.ProjectKey is not null && !project.ProjectKey.Contains(payload.Issue.Fields.Project.Key))
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
            [WebhookParameter] ProjectIssueInput project,
            [WebhookParameter] LabelsOptionalInput labels)
        {
            var payload = DeserializePayload(request);
            var issueTypeItem = payload.Changelog.Items.FirstOrDefault(item => item.FieldId == "issuetype");

            if ((issueTypeItem is null && payload.WebhookEvent == "jira:issue_updated")
                || (project.ProjectKey is not null && !project.ProjectKey.Contains(payload.Issue.Fields.Project.Key))
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
            [WebhookParameter] ProjectIssueInput project,
            [WebhookParameter] LabelsOptionalInput labels)
        {
            var payload = DeserializePayload(request);
            var priorityItem = payload.Changelog.Items.FirstOrDefault(item => item.FieldId == "priority");

            if (priorityItem == null
                || (project.ProjectKey is not null && !project.ProjectKey.Contains(payload.Issue.Fields.Project.Key))
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

        [Webhook("On file attached to issue", typeof(IssueUpdatedHandler),
            Description = "This webhook is triggered when a file is attached to an issue.")]
        public async Task<WebhookResponse<IssueAttachmentResponse>> OnFileAttachedToIssue(WebhookRequest request,
            [WebhookParameter] IssueInput issue,
            [WebhookParameter] ProjectIssueInput project)
        {
            var payload = DeserializePayload(request);
            var attachmentItem = payload.Changelog.Items.FirstOrDefault(item => item.FieldId == "attachment");

            if (attachmentItem is null
                || (project.ProjectKey is not null && !project.ProjectKey.Contains(payload.Issue.Fields.Project.Key))
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

        [Webhook("On issue status changed", typeof(IssueUpdatedHandler),Description = "This webhook is triggered when issue status is changed.")]
        public async Task<WebhookResponse<IssueResponse>> OnIssueStatusChanged(WebhookRequest request,
            [WebhookParameter] ProjectIdentifier project,
            [WebhookParameter] OptionalStatusInput status,
            [WebhookParameter] IssueInput issue,
            [WebhookParameter] LabelsOptionalInput labels,
            [WebhookParameter] IssueTypeOptionalInput IssueType,
            [WebhookParameter] string? summary)
        {
            var payload = DeserializePayload(request);
            var statusItem = payload.Changelog.Items.FirstOrDefault(item => item.FieldId == "status");

            if (statusItem is null
                || (project.ProjectKey is not null && !project.ProjectKey.Equals(payload.Issue.Fields.Project.Key))
                || (status.StatusId is not null && payload.Issue.Fields.Status.Id != status.StatusId)
                || (issue.IssueKey is not null && !issue.IssueKey.Equals(payload.Issue.Key))
                || (!String.IsNullOrEmpty(summary) && !payload.Issue.Fields.Summary.Contains(summary))
                || (IssueType?.IssueTypeId is not null && payload.Issue.Fields.IssueType.Id != IssueType.IssueTypeId))
                return new WebhookResponse<IssueResponse>
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight
                };

            var issueResponse = CreateIssueResponse(payload, labels);
            return issueResponse;
        }

        [Webhook("On issues reach status", typeof(IssueUpdatedHandler),
          Description = "Triggered when ALL specified issues reach the given status. Emits preflight until all are in that status.")]
        public async Task<WebhookResponse<IssuesReachedStatusResponse>> OnIssuesReachStatus(
          WebhookRequest request, [WebhookParameter] ProjectIdentifier projectId,[WebhookParameter] IssuesReachStatusInput input)
        {
            InvocationContext.Logger?.LogInformation("[Jira][OnIssuesReachStatus] Invoke of webhook", null);
            var payload = DeserializePayload(request);
            InvocationContext.Logger?.LogInformation(
                $"[Jira][OnIssuesReachStatus] Payload of webhook - {Newtonsoft.Json.JsonConvert.SerializeObject(payload, Newtonsoft.Json.Formatting.Indented)}",
                null);

            var statusItem = payload.Changelog?.Items?.FirstOrDefault(i =>
                string.Equals(i.Field, "status", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(i.FieldId, "status", StringComparison.OrdinalIgnoreCase));

            if (statusItem is null)
                InvocationContext.Logger?.LogInformation(
                    "[Jira][OnIssuesReachStatus] No 'status' item in changelog; will verify current status by fetching issues.", null);

            if (!string.IsNullOrWhiteSpace(projectId?.ProjectKey) &&
                !string.Equals(projectId.ProjectKey, payload.Issue.Fields.Project.Key, StringComparison.OrdinalIgnoreCase))
            {
                InvocationContext.Logger?.LogInformation(
                    $"[Jira][OnIssuesReachStatus] Payload project '{payload.Issue.Fields.Project.Key}' != filter '{projectId.ProjectKey}'. Continuing (multi-project allowed).", null);
            }

            var normalizedKeys = new HashSet<string>(
                (input.IssueKeys ?? Enumerable.Empty<string>())
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Select(k => k.Trim().ToUpperInvariant())
            );
            if (normalizedKeys.Count == 0) return Preflight<IssuesReachedStatusResponse>();

            var changedKey = payload.Issue.Key?.Trim()?.ToUpperInvariant();
            InvocationContext.Logger?.LogInformation(
                $"[Jira][OnIssuesReachStatus] changedKey={changedKey}; inputKeys=[{string.Join(", ", normalizedKeys)}]",
                null);

            if (changedKey is null || !normalizedKeys.Contains(changedKey))
                return Preflight<IssuesReachedStatusResponse>();

            var rawStatus = (input.Status ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(rawStatus))
                return Preflight<IssuesReachedStatusResponse>();

            string? targetStatusId = null;
            string targetStatusName;

            if (LooksLikeId(rawStatus))
            {
                targetStatusId = rawStatus;
                targetStatusName = await GetStatusNameById(rawStatus);
            }
            else
            {
                targetStatusName = rawStatus;
            }

            var current = payload.Issue.Fields?.Status;
            if ((targetStatusId != null && !string.Equals(current?.Id, targetStatusId, StringComparison.OrdinalIgnoreCase)) ||
                (targetStatusId == null && !string.Equals(current?.Name, targetStatusName, StringComparison.OrdinalIgnoreCase)))
                return Preflight<IssuesReachedStatusResponse>();

            var issues = new List<IssueWrapper>();
            var missing = new List<string>();
            foreach (var key in normalizedKeys)
            {
                try
                {
                    var issue = await GetIssue(key);
                    issues.Add(issue);
                }
                catch (Exception ex)
                {
                    InvocationContext.Logger?.LogInformation(
                        $"[Jira][OnIssuesReachStatus] Can't load issue '{key}': {ex.Message}", null);
                    missing.Add(key);
                }
            }

            if (missing.Count > 0)
            {
                InvocationContext.Logger?.LogInformation(
                    $"[Jira][OnIssuesReachStatus] Missing/unavailable issues: {string.Join(", ", missing)}",
                    null);
                return Preflight<IssuesReachedStatusResponse>();
            }

            var notInTarget = new List<string>();
            foreach (var i in issues)
            {
                var cur = i.Fields?.Status;
                var ok =
                    (!string.IsNullOrEmpty(targetStatusId) &&
                     string.Equals(cur?.Id, targetStatusId, StringComparison.OrdinalIgnoreCase))
                    ||
                    (string.IsNullOrEmpty(targetStatusId) &&
                     string.Equals(cur?.Name, targetStatusName, StringComparison.OrdinalIgnoreCase));

                if (!ok)
                    notInTarget.Add($"{i.Key} [{cur?.Id}:{cur?.Name}]");
            }

            if (notInTarget.Count > 0)
            {
                InvocationContext.Logger?.LogInformation(
                    $"[Jira][OnIssuesReachStatus] Still not in target '{targetStatusName}' (id: {targetStatusId ?? "n/a"}): {string.Join(", ", notInTarget)}",
                    null);
                return Preflight<IssuesReachedStatusResponse>();
            }

            var results = issues.Select(MapToIssueResponse).ToList();

            return new WebhookResponse<IssuesReachedStatusResponse>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                ReceivedWebhookRequestType = WebhookRequestType.Default,
                Result = new IssuesReachedStatusResponse { Issues = results }
            };
        }

        private async Task<IssueWrapper> GetIssue(string key)
        {
            var getReq = new JiraRequest($"/issue/{key}", Method.Get);
            getReq.AddQueryParameter("fields",
                "summary,status,issuetype,priority,assignee,project,labels,duedate,reporter,subtasks");
            return await Client.ExecuteWithHandling<IssueWrapper>(getReq);
        }

        private static WebhookResponse<T> Preflight<T>() where T : class =>
            new()
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };

        private IssueResponse MapToIssueResponse(IssueWrapper i)
        {
            DateTime due = DateTime.MinValue;
            if (!string.IsNullOrEmpty(i.Fields?.DueDate) && DateTime.TryParse(i.Fields.DueDate, out var d))
                due = d;

            return new IssueResponse
            {
                IssueKey = i.Key,
                ProjectKey = i.Fields?.Project?.Key,
                Summary = i.Fields?.Summary,
                Description = ExtractPlainText(i.Fields?.Description),
                IssueType = i.Fields?.IssueType?.Name,
                Priority = i.Fields?.Priority?.Name,
                AssigneeName = i.Fields?.Assignee?.DisplayName,
                AssigneeAccountId = i.Fields?.Assignee?.AccountId,
                Status = i.Fields?.Status?.Name,
                Attachments = i.Fields?.Attachment?.ToList() ?? new List<AttachmentDto>(),
                DueDate = due,
                Labels = i.Fields?.Labels ?? new List<string>()
            };
        }

        private static bool LooksLikeId(string s) => s.All(char.IsDigit);

        private async Task<string> GetStatusNameById(string id)
        {
            var req = new JiraRequest($"/status/{id}", Method.Get);
            var dto = await Client.ExecuteWithHandling<SimpleStatusDto>(req);
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                throw new PluginApplicationException($"Status with id '{id}' not found.");
            return dto.Name;
        }

        private static string? ExtractPlainText(Description? desc)
        {
            if (desc == null || desc.Content == null) return null;

            var parts = new List<string>();
            void Walk(IEnumerable<ContentElement> nodes)
            {
                foreach (var n in nodes)
                {
                    if (!string.IsNullOrEmpty(n.Text)) parts.Add(n.Text);
                    if (n.Content != null && n.Content.Count > 0) Walk(n.Content);
                }
            }
            Walk(desc.Content);
            var text = string.Join("", parts).Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }

        private WebhookPayload DeserializePayload(WebhookRequest request)
        {
            try
            {
                var payload = JsonConvert.DeserializeObject<WebhookPayload>(request.Body.ToString()!, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                }) ?? throw new InvalidCastException("Failed to deserialize the webhook payload. Payload is null.");
                
                return payload;
            }
            catch (JsonSerializationException ex)
            {
                throw new InvalidCastException($"Failed to deserialize the webhook payload. Body: {request.Body}; Body null: {request.Body == null}; Got exception: {ex.Message};");
            }
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

            if (labelsInput.LabelsDropDown is not null && labelsInput.LabelsDropDown.Any())
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
                    DueDate = !string.IsNullOrEmpty(issue.Fields.DueDate) && DateTime.TryParse(issue.Fields.DueDate, out var dueDate)
                        ? dueDate
                        : DateTime.MinValue,
                    Labels = issue.Fields.Labels
                }
            };
        }
    }
}