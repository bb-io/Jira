using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Webhooks.Inputs;
using Apps.Jira.Webhooks.Responses;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using RestSharp;

namespace Apps.Jira.Webhooks.Handlers.IssueHandlers
{
    public class IssueUpdatedHandler(
        InvocationContext invocationContext,
        [WebhookParameter] ProjectIdentifier projectId,
        [WebhookParameter] IssuesReachStatusInput input)
        : BaseWebhookHandler(invocationContext, SubscriptionEvent),IAfterSubscriptionWebhookEventHandler<IssuesReachedStatusResponse>
    {
        private static readonly string[] SubscriptionEvent = { "jira:issue_updated" };

        public async Task<AfterSubscriptionEventResponse<IssuesReachedStatusResponse>> OnWebhookSubscribedAsync()
        {
            InvocationContext.Logger?.LogInformation("[Jira][OnIssuesReachStatus][AfterSub] Start after-subscription check ", null);

            var normalizedKeys = new HashSet<string>(
                (input.IssueKeys ?? Enumerable.Empty<string>())
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Select(k => k.Trim().ToUpperInvariant())
            );

            if (normalizedKeys.Count == 0)
            {
                InvocationContext.Logger?.LogInformation("[Jira][OnIssuesReachStatus][AfterSub] No issue keys provided → nothing to emit", null);
                return null!;
            }

            var rawStatuses = (input.Statuses ?? Enumerable.Empty<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();

            if (rawStatuses.Count == 0)
            {
                InvocationContext.Logger?.LogInformation("[Jira][OnIssuesReachStatus][AfterSub] No statuses provided → nothing to emit", null);
                return null!;
            }

            var (allowedIds, allowedNames) = await ResolveStatusesAsync(rawStatuses);

            var issues = new List<IssueWrapper>();
            foreach (var key in normalizedKeys)
            {
                try
                {
                    var issue = await GetIssue(key);
                    issues.Add(issue);
                }
                catch (Exception ex)
                {
                    InvocationContext.Logger?.LogInformation($"[Jira][OnIssuesReachStatus][AfterSub] Can't load issue '{key}': {ex.Message}", null);
                    return null!;
                }
            }

            if (!string.IsNullOrWhiteSpace(projectId?.ProjectKey))
            {
                var mismatch = issues.FirstOrDefault(i =>
                    !string.Equals(i.Fields?.Project?.Key, projectId.ProjectKey, StringComparison.OrdinalIgnoreCase));

                if (mismatch != null)
                {
                    InvocationContext.Logger?.LogInformation(
                        $"[Jira][OnIssuesReachStatus][AfterSub] Found issue from another project '{mismatch.Fields?.Project?.Key}' while filter is '{projectId.ProjectKey}' → skip emit",
                        null);
                    return null!;
                }
            }

            var notInAllowed = new List<string>();
            foreach (var i in issues)
            {
                var s = i.Fields?.Status;
                if (!IsAllowedStatus(s?.Id, s?.Name, allowedIds, allowedNames))
                    notInAllowed.Add($"{i.Key} [{s?.Id}:{s?.Name}]");
            }

            if (notInAllowed.Count > 0)
            {
                InvocationContext.Logger?.LogInformation(
                    $"[Jira][OnIssuesReachStatus][AfterSub] Not all issues are in allowed statuses → {string.Join(", ", notInAllowed)}",
                    null);
                return null!;
            }

            var results = issues.Select(MapToIssueResponse).ToList();
            return new AfterSubscriptionEventResponse<IssuesReachedStatusResponse>
            {
                Result = new IssuesReachedStatusResponse { Issues = results }
            };
        }
        private async Task<IssueWrapper> GetIssue(string key)
        {
            var client = new JiraClient(InvocationContext.AuthenticationCredentialsProviders);
            var req = new JiraRequest($"/issue/{key}", Method.Get);
            req.AddQueryParameter("fields",
                "summary,status,issuetype,priority,assignee,project,labels,duedate,reporter,subtasks,description,attachment");
            return await client.ExecuteWithHandling<IssueWrapper>(req);
        }

        private async Task<string> GetStatusNameById(string id)
        {
            var client = new JiraClient(InvocationContext.AuthenticationCredentialsProviders);
            var req = new JiraRequest($"/status/{id}", Method.Get);
            var dto = await client.ExecuteWithHandling<SimpleStatusDto>(req);
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                throw new PluginApplicationException($"Status with id '{id}' not found.");
            return dto.Name;
        }

        private async Task<(HashSet<string> ids, HashSet<string> names)> ResolveStatusesAsync(IEnumerable<string> raw)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var token in raw)
            {
                if (token.All(char.IsDigit))
                {
                    ids.Add(token);
                    try
                    {
                        var name = await GetStatusNameById(token);
                        if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                    }
                    catch { }
                }
                else
                {
                    names.Add(token);
                }
            }

            return (ids, names);
        }

        private static bool IsAllowedStatus(string? id, string? name, HashSet<string> allowedIds, HashSet<string> allowedNames)
        {
            var idOk = !string.IsNullOrEmpty(id) && allowedIds.Contains(id);
            var nameOk = !string.IsNullOrEmpty(name) && allowedNames.Contains(name);
            return idOk || nameOk;
        }

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
    }
}