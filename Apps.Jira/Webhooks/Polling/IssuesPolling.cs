using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Webhooks.Inputs;
using Apps.Jira.Webhooks.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.Sdk.Common.Webhooks;
using RestSharp;

namespace Apps.Jira.Webhooks.Polling
{
    [PollingEventList]
    public class IssuesPolling(InvocationContext invocationContext) : JiraInvocable(invocationContext)
    {
        [PollingEvent("On issues reach status (polling)")]
        public async Task<IssuesReachedStatusResponse> OnIssuesReachStatusPolling(PollingEventRequest<PollingMemory> request,
            [PollingEventParameter] ProjectIdentifier projectId, [PollingEventParameter] IssuesReachStatusInput input)
        {
            request.Memory ??= new PollingMemory();
            request.Memory.LastPollingTime = DateTime.UtcNow;


            var normalizedKeys = new HashSet<string>(
                (input.IssueKeys ?? Enumerable.Empty<string>())
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Select(k => k.Trim().ToUpperInvariant())
            );
            if (normalizedKeys.Count == 0)
            {
                return null;
            }

            var rawStatuses = (input.Statuses ?? Enumerable.Empty<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();
            if (rawStatuses.Count == 0)
            {
                return null;
            }

            var (allowedIds, allowedNames) = await ResolveStatusesAsync(rawStatuses);

            var issues = new List<IssueWrapper>();
            foreach (var key in normalizedKeys)
            {
                try
                {
                    issues.Add(await GetIssue(key));
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            if (!string.IsNullOrWhiteSpace(projectId?.ProjectKey))
            {
                var mismatch = issues.FirstOrDefault(i =>
                    !string.Equals(i.Fields?.Project?.Key, projectId.ProjectKey, StringComparison.OrdinalIgnoreCase));

                if (mismatch != null)
                {
                    return null;
                }
            }

            var notInAllowed = issues
                .Where(i =>
                {
                    var s = i.Fields?.Status;
                    return !IsAllowedStatus(s?.Id, s?.Name, allowedIds, allowedNames);
                })
                .Select(i => new { i.Key, id = i.Fields?.Status?.Id, name = i.Fields?.Status?.Name })
                .ToList();

            if (notInAllowed.Count > 0)
            {
                return null;
            }

            var results = issues.Select(MapToIssueResponse).ToList();

            return new IssuesReachedStatusResponse { Issues = results };
        }


        private async Task<IssueWrapper> GetIssue(string key)
        {
            var req = new JiraRequest($"/issue/{key}", Method.Get);
            req.AddQueryParameter("fields",
                "summary,status,issuetype,priority,assignee,project,labels,duedate,reporter,subtasks,description,attachment");
            return await Client.ExecuteWithHandling<IssueWrapper>(req);
        }

        private async Task<string> GetStatusNameById(string id)
        {
            var req = new JiraRequest($"/status/{id}", Method.Get);
            var dto = await Client.ExecuteWithHandling<SimpleStatusDto>(req);
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
            => (!string.IsNullOrEmpty(id) && allowedIds.Contains(id))
            || (!string.IsNullOrEmpty(name) && allowedNames.Contains(name));

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
            if (desc?.Content == null) return null;

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
