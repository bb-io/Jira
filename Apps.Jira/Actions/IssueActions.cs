using Apps.Jira.DataSourceHandlers;
using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using Apps.Jira.Utils;
using Apps.Jira.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System.Globalization;
using System.Web;
using Method = RestSharp.Method;
namespace Apps.Jira.Actions;

[ActionList("Issues")]
public class IssueActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : JiraInvocable(invocationContext)
{
    #region GET

    [Action("Get issue", Description = "Get the specified issue.")]
    public async Task<IssueDto> GetIssueByKey([ActionParameter] IssueIdentifier input)
    {
        if (input == null || string.IsNullOrEmpty(input.IssueKey))
        {
            throw new PluginMisconfigurationException("IssueKey can not be null or empty.");
        }

        var request = new JiraRequest($"/issue/{input.IssueKey}", Method.Get);
        var issue = await Client.ExecuteWithHandling<IssueWrapper>(request);
        return new IssueDto(issue);
    }

    [Action("Search issues", Description =
        "Returns issues that meet the provided criteria.")]
    public async Task<IssuesResponse> ListRecentlyCreatedIssues(
        [ActionParameter] ProjectIdentifier project,
        [ActionParameter] ListRecentlyCreatedIssuesRequest listRequest,
        [ActionParameter][Display("Custom JQL conditions")] string? customJql)
    {
        List<string> jqlConditions = [
            $"project={project.ProjectKey}"
        ];

        if (listRequest.Hours.HasValue)
        {
            jqlConditions.Add($"Created >= -{listRequest.Hours}h");
        }

        if (listRequest.Labels != null && listRequest.Labels.Any())
        {
            var labelsQuotedCommaSeparatedList = '"' + string.Join("\", \"", listRequest.Labels.Where(l => !string.IsNullOrWhiteSpace(l))) + '"';
            jqlConditions.Add($"labels in ({labelsQuotedCommaSeparatedList})");
        }

        if (listRequest.Versions != null && listRequest.Versions.Any())
        {
            var versionsQuotedCommaSeparatedList = '"' + string.Join("\", \"", listRequest.Versions.Where(v => !string.IsNullOrWhiteSpace(v))) + '"';
            jqlConditions.Add($"fixVersion in ({versionsQuotedCommaSeparatedList})");
        }

        if (!string.IsNullOrWhiteSpace(listRequest.ParentIssue))
        {
            jqlConditions.Add($"parent = {listRequest.ParentIssue.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(customJql))
        {
            jqlConditions.Add($"({customJql})");
        }

        var request = new JiraRequest("/search/jql", Method.Get);
        request.AddQueryParameter("jql", string.Join(" and ", jqlConditions));
        request.AddQueryParameter("fields","id,key,summary,status,priority,assignee,reporter,project,description,labels,subtasks,duedate,parent");

        var issues = await Client.ExecuteWithHandling<IssuesWrapper>(request);

        return new IssuesResponse
        {
            Issues = issues.Issues.Select(i => new IssueDto(i)),
            Count = issues.Issues.Count()
        };
    }

    [Action("Clone issue", Description = "Clone an existing issue into a new issue")]
    public async Task<IssueDto> CloneIssue([ActionParameter] IssueIdentifier sourceIssue,
       [ActionParameter] CloneIssueRequest input)
    {
        var getWrapReq = new JiraRequest($"/issue/{sourceIssue.IssueKey}", Method.Get);
        var original = await Client.ExecuteWithHandling<IssueWrapper>(getWrapReq);

        var getRawReq = new JiraRequest($"/issue/{sourceIssue.IssueKey}", Method.Get);
        var originalRaw = await Client.ExecuteWithHandling<JObject>(getRawReq);
        var originalFieldsObj = (JObject?)originalRaw["fields"] ?? new JObject();

        var createSchemas = await GetCreateFieldSchemas(original.Fields.Project.Key, original.Fields.IssueType.Id);

        var fields = new JObject();

        if (!string.IsNullOrWhiteSpace(original.Fields.Project.Id))
            fields["project"] = new JObject { ["id"] = original.Fields.Project.Id };
        else
            fields["project"] = new JObject { ["key"] = original.Fields.Project.Key };

        fields["issuetype"] = new JObject { ["id"] = original.Fields.IssueType.Id };

        fields["summary"] = input.NewSummary ?? original.Fields.Summary;

        if (original.Fields.Priority != null)
            fields["priority"] = new JObject { ["id"] = original.Fields.Priority.Id };

        if (original.Fields.Labels != null && original.Fields.Labels.Any())
            fields["labels"] = new JArray(original.Fields.Labels);

        if (!string.IsNullOrWhiteSpace(input.AssigneeName))
            fields["assignee"] = new JObject { ["accountId"] = input.AssigneeName };
        else if (original.Fields.Assignee?.Active == true && !string.IsNullOrWhiteSpace(original.Fields.Assignee.AccountId))
            fields["assignee"] = new JObject { ["accountId"] = original.Fields.Assignee.AccountId };

        var me = await GetCurrentUserAsync();
        var reporterId =
            (!string.IsNullOrWhiteSpace(input.ReporterName) ? input.ReporterName : null) ??
            (original.Fields.Reporter?.Active == true ? original.Fields.Reporter?.AccountId : null) ??
            me.AccountId;

        if (string.IsNullOrWhiteSpace(reporterId))
            throw new PluginApplicationException("Reporter is required but no suitable accountId found.");

        fields["reporter"] = new JObject { ["accountId"] = reporterId };

        if (!string.IsNullOrWhiteSpace(original.Fields.DueDate) &&
            DateTime.TryParse(original.Fields.DueDate, out var dd))
            fields["duedate"] = dd.ToString("yyyy-MM-dd");

        if (original.Fields.Parent != null)
            fields["parent"] = new JObject { ["key"] = original.Fields.Parent.Key };

        if (originalFieldsObj["description"] is JToken adf &&
            adf.Type != JTokenType.Null &&
            createSchemas.ContainsKey("description"))
        {
            fields["description"] = adf.DeepClone();
        }

        foreach (var prop in originalFieldsObj.Properties())
        {
            var key = prop.Name;
            if (!key.StartsWith("customfield_", StringComparison.OrdinalIgnoreCase))
                continue;

            var value = prop.Value;
            if (value is null || value.Type == JTokenType.Null)
                continue;

            if (!createSchemas.TryGetValue(key, out var schema))
                continue;

            var customId = (schema.Custom ?? string.Empty).ToLowerInvariant();
            if (customId.Contains("gh-lexo-rank") || customId.Contains("gh-simplified-rank") || customId.Contains("greenhopper"))
                continue;

            var normalized = NormalizeCustomFieldValue(value, schema);
            if (normalized is not null)
                fields[key] = JToken.FromObject(normalized);
        }

        var skipNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "rank","rankBeforeIssue","rankAfterIssue",
            "attachment","votes","watches","worklog","comment",
            "timeoriginalestimate","timeestimate","timespent",
            "aggregatetimeoriginalestimate","aggregatetimeestimate","aggregatetimespent",
            "timetracking","lastViewed","created","updated","resolution","resolutiondate",
            "statuscategorychangedate","creator","progress","aggregateprogress"
        };

        var toRemove = new List<string>();
        foreach (var p in fields.Properties())
        {
            var name = p.Name;
            if (skipNames.Contains(name) || name.IndexOf("rank", StringComparison.OrdinalIgnoreCase) >= 0)
                toRemove.Add(name);
        }
        foreach (var n in toRemove) fields.Remove(n);

        var body = new JObject { ["fields"] = fields };
        var createReq = new JiraRequest("/issue", Method.Post).AddJsonBody(body.ToString());

        CreatedIssueDto created;
        try
        {
            created = await Client.ExecuteWithHandling<CreatedIssueDto>(createReq);
        }
        catch (Exception ex)
        {
            throw new PluginApplicationException($"Failed to clone issue {sourceIssue.IssueKey}: {ex.Message}", ex);
        }

       await LinkIssuesCloners(created.Key, sourceIssue.IssueKey); 


        if (input.CopyStatus == true)
        {await TransitionIssueToStatus(created.Key, original.Fields.Status.Id);

        }

        return await GetIssueByKey(new IssueIdentifier { IssueKey = created.Key });
    }

    private async Task<Dictionary<string, CreateMetaField>> GetCreateFieldSchemas(string projectKey, string issueTypeId)
    {
        var req = new JiraRequest($"/issue/createmeta", Method.Get)
        .AddQueryParameter("projectKeys", projectKey)
        .AddQueryParameter("issuetypeIds", issueTypeId)
        .AddQueryParameter("expand", "projects.issuetypes.fields");

        var resp = await Client.ExecuteWithHandling<JObject>(req);

        var dict = new Dictionary<string, CreateMetaField>(StringComparer.OrdinalIgnoreCase);
        var fields = resp?["projects"]?[0]?["issuetypes"]?[0]?["fields"] as JObject;
        if (fields is null) return dict;

        foreach (var prop in fields.Properties())
        {
            if (prop.Value is not JObject f) continue;

            dict[prop.Name] = new CreateMetaField
            {
                Name = f["name"]?.ToString(),
                Key = prop.Name,
                SchemaType = f.SelectToken("schema.type")?.ToString(),
                SchemaItems = f.SelectToken("schema.items")?.ToString(),
                Custom = f.SelectToken("schema.custom")?.ToString(),
                Required = f["required"]?.Value<bool>() ?? false
            };
        }
        return dict;
    }

    private object? NormalizeCustomFieldValue(JToken srcValue, CreateMetaField schema)
    {
        if (srcValue.Type == JTokenType.Null) return null;

        var type = schema.SchemaType;
        var items = schema.SchemaItems;

        switch (type)
        {
            case "user":
                {
                    var accountId = srcValue["accountId"]?.ToString();
                    return string.IsNullOrWhiteSpace(accountId) ? null : new { accountId };
                }

            case "array":
                {
                    if (srcValue is not JArray arr) return null;
                    switch (items)
                    {
                        case "user":
                            return arr.Select(u =>
                            {
                                var aid = u?["accountId"]?.ToString();
                                return string.IsNullOrWhiteSpace(aid) ? null : new { accountId = aid };
                            }).Where(x => x is not null).ToArray();

                        case "option":
                            return arr.Select<JToken, object?>(opt =>
                            {
                                var id = opt?["id"]?.ToString();
                                var value = opt?["value"]?.ToString();
                                return !string.IsNullOrEmpty(id) ? new { id } :
                                       !string.IsNullOrEmpty(value) ? new { value } : null;
                            }).Where(x => x is not null).ToArray();

                        case "version":
                        case "component":
                            return arr.Select(v =>
                            {
                                var id = v?["id"]?.ToString();
                                return !string.IsNullOrEmpty(id) ? new { id } : null;
                            }).Where(x => x is not null).ToArray();

                        default:
                            return arr.Select(a => (a as JValue)?.Value).ToArray();
                    }
                }

            case "option":
                {
                    var id = srcValue["id"]?.ToString();
                    var value = srcValue["value"]?.ToString();
                    if (!string.IsNullOrEmpty(id)) return new { id };
                    if (!string.IsNullOrEmpty(value)) return new { value };
                    return null;
                }

            case "version":
            case "component":
                {
                    var id = srcValue["id"]?.ToString();
                    return !string.IsNullOrEmpty(id) ? new { id } : null;
                }

            case "number":
                {
                    if (srcValue is JValue v && (v.Type == JTokenType.Integer || v.Type == JTokenType.Float))
                        return v.Value;
                    if (decimal.TryParse(srcValue.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                        return dec;
                    return null;
                }

            case "date":
            case "datetime":
                {
                    return srcValue.Type == JTokenType.String ? srcValue.ToString() : null;
                }

            default:
                {
                    if (srcValue is JValue sv) return sv.Value;
                    return srcValue.ToObject<object>();
                }
        }
    }

    private async Task<(string? AccountId, string? Name)> GetCurrentUserAsync()
    {
        var req = new JiraRequest("/myself", Method.Get);
        var me = await Client.ExecuteWithHandling<JObject>(req);
        return (me["accountId"]?.ToString(), me["name"]?.ToString() ?? me["key"]?.ToString());
    }

    private async Task LinkIssuesCloners(string newIssueKey, string sourceKey)
    {
        var payload = new
        {
            type = new { name = "Cloners" },
            outwardIssue = new { key = sourceKey },
            inwardIssue = new { key = newIssueKey }
        };

        var req = new JiraRequest("/issueLink", Method.Post).AddJsonBody(payload);
        await Client.ExecuteWithHandling(req);
    }

    private async Task TransitionIssueToStatus(string issueKey, string statusId)
    {
        var get = new JiraRequest($"/issue/{issueKey}/transitions", Method.Get);
        var transitions = await Client.ExecuteWithHandling<TransitionsResponse>(get);

        var target = transitions.Transitions.FirstOrDefault(t => t.To.Id == statusId);
        if (target != null)
        {
            var post = new JiraRequest($"/issue/{issueKey}/transitions", Method.Post)
                .AddJsonBody(new { transition = new { id = target.Id } });
            await Client.ExecuteWithHandling(post);
        }
    }

    public async Task LinkIssue([ActionParameter] LinkIssueRequest input)
    {
        var requestBody = new
        {
            inwardIssue = new
            {
                key = input.InwardIssueKey
            },
            outwardIssue = new
            {
                key = input.OutwardIssueKey
            },
            type = new
            {
                name = input.LinkTypeName
            },
            comment = new
            {
                body = input.Comment ?? "Linked via API"
            }
        };

        var request = new JiraRequest("/issueLink", Method.Post)
            .WithJsonBody(requestBody);
        await Client.ExecuteWithHandling(request);
    }

    [Action("Find issue", Description = "Find the first issue that matches given conditions. Allows appending custom JQL conditions.")]
    public async Task<IssueDto> FindIssue(
    [ActionParameter] [Display("Parent issue")][DataSource(typeof(IssueDataSourceHandler))]string? parentIssue,
    [ActionParameter] [Display("Summary")] string? issueName,
    [ActionParameter] ProjectIdentifier project,
    [ActionParameter][Display("Custom JQL conditions")] string? customJql = null)
    {
        var jqlConditions = new List<string>();

        if (project != null)
        {
            jqlConditions.Add($"project={project.ProjectKey}");
        }

        if (!string.IsNullOrWhiteSpace(parentIssue))
        {
            jqlConditions.Add($"(parent = {parentIssue.Trim()} OR \"Epic Link\" = {parentIssue.Trim()})");
        }

        if (!string.IsNullOrWhiteSpace(issueName))
        {
            jqlConditions.Add($"summary ~ \"{issueName.Trim()}\"");
        }

        if (!string.IsNullOrWhiteSpace(customJql))
        {
            jqlConditions.Add($"({customJql})");
        }

        var request = new JiraRequest("/search/jql", Method.Get);
        request.AddQueryParameter("jql", string.Join(" AND ", jqlConditions));
        request.AddQueryParameter("fields", "id,key,summary,status,priority,assignee,reporter,project,description,labels,subtasks,duedate,parent");
        request.AddQueryParameter("maxResults", "1");

        var issues = await Client.ExecuteWithHandling<IssuesWrapper>(request);

        var firstIssue = issues.Issues.FirstOrDefault();
        return firstIssue != null ? new IssueDto(firstIssue) : null;
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
    public async Task<IssueTypeDto> GetIssueTypeDetails([ActionParameter] ProjectIdentifier projectIdentifier,
        [Display("Type name")] [ActionParameter] string TypeName)
    {
        var getProjectRequest = new JiraRequest($"/project/{projectIdentifier.ProjectKey}", Method.Get);
        var project = await Client.ExecuteWithHandling<ProjectDto>(getProjectRequest);

        var getIssueTypesRequest = new JiraRequest("/issuetype", Method.Get);
        var issueTypes = await Client.ExecuteWithHandling<IEnumerable<IssueTypeDto>>(getIssueTypesRequest);
        try
        {
            return issueTypes.Where(type =>
                    type.Scope is null || type.Scope?.Type == "PROJECT" && type.Scope.Project!.Id == project.Id)
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
        var projectRequest = new JiraRequest($"/project/{project.ProjectKey}", Method.Get);
        var projectResponse = await Client.ExecuteWithHandling<DetailedProjectDto>(projectRequest);
        var validIssueTypes = projectResponse.IssueTypes.Select(t => t.Id).ToList();
        if (!validIssueTypes.Contains(input.IssueTypeId))
        {
            throw new PluginMisconfigurationException($"Invalid issue type ID: {input.IssueTypeId}. Valid issue type IDs for project {project.ProjectKey} are: {string.Join(", ", validIssueTypes)}");
        }

        var fields = new Dictionary<string, object>
        {
            { "project", new { key = project.ProjectKey } },
            { "summary", input.Summary },
            {
                "description", new
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

        if (!string.IsNullOrEmpty(input.OriginalEstimate))
        {
            fields.Add("timetracking", new { originalEstimate = input.OriginalEstimate});
        }

        if (input.DueDate.HasValue)
        {
            fields.Add("duedate", input.DueDate.Value.ToString("yyyy-MM-dd"));
        }

        if (!string.IsNullOrEmpty(input.Reporter))
        {
            fields.Add("reporter", new { id = input.Reporter });
        }

        if (!string.IsNullOrEmpty(input.ParentIssueKey))
            fields.Add("parent", new { key = input.ParentIssueKey });

        if (input.Labels is { } labels && labels.Any())
        {
            var normalized = labels
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (normalized.Length > 0)
                fields.Add("labels", normalized);
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


    [Action("Move issues to sprint", Description = "Moves issues to a specific sprint")]
    public async Task<MoveIssuesToSprintResponse> MoveIssuesToSprint(
    [ActionParameter] MoveIssuesToSprintRequest input)
    {
        var authenticationProviders = InvocationContext.AuthenticationCredentialsProviders;
        var agileClient = new JiraClient(authenticationProviders, "agile");

        var request = new JiraRequest($"/sprint/{input.SprintId}/issue", Method.Post)
         .WithJsonBody(new
         {
             issues = input.Issues,
             rankAfterIssue = input.RankAfterIssue,
             rankBeforeIssue = input.RankBeforeIssue,
             rankCustomFieldId = input.RankCustomFieldId
         });

        try
        {
            var response = await agileClient.ExecuteWithHandling(request);

            return new MoveIssuesToSprintResponse
            {
                Success = true,
                Message = "Issues moved successfully."
            };
        }
        catch (Exception ex)
        {
            return new MoveIssuesToSprintResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
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

            var assigneeRequest = new JiraRequest($"/issue/{issue.IssueKey}/assignee", Method.Put)
                .WithJsonBody(new { accountId });
            await Client.ExecuteWithHandling(assigneeRequest);
        }


        if (input.Summary != null || input.Description != null || input.IssueTypeId != null ||
            !string.IsNullOrEmpty(input.OriginalEstimate) || input.DueDate.HasValue || !string.IsNullOrEmpty(input.Reporter))
        {
            var fieldsUpdate = new Dictionary<string, object>();

            if (input.Summary != null)
                fieldsUpdate.Add("summary", input.Summary);

            if (input.Description != null)
            {
                var descriptionJson = MarkdownToJiraConverter.ConvertMarkdownToJiraDoc(input.Description);
                fieldsUpdate.Add("description", descriptionJson);
            }

            if (input.IssueTypeId != null)
                fieldsUpdate.Add("issuetype", new { id = input.IssueTypeId });


            if (!string.IsNullOrEmpty(input.OriginalEstimate))
            {
                fieldsUpdate.Add("timetracking", new { originalEstimate = input.OriginalEstimate});
            }

            if (input.DueDate.HasValue)
            {
                fieldsUpdate.Add("duedate", input.DueDate.Value.ToString("yyyy-MM-dd"));
            }

            if (!string.IsNullOrEmpty(input.Reporter))
            {
                fieldsUpdate.Add("reporter", new { id = input.Reporter });
            }

            var endpoint = $"/issue/{issue.IssueKey}";
            if (input.OverrideScreenSecurity.HasValue)
            {
                endpoint += $"?overrideScreenSecurity={input.OverrideScreenSecurity.Value}";
            }
            if (input.NotifyUsers.HasValue)
            {
                endpoint = endpoint + (input.OverrideScreenSecurity.HasValue ? "&" : "?") +
                           $"notifyUsers={input.NotifyUsers}";
            }

            var updateRequest = new JiraRequest(endpoint, Method.Put)
                .WithJsonBody(new { fields = fieldsUpdate },
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            await Client.ExecuteWithHandling(updateRequest);
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

    [Action("Append to issue description", Description = "Appends additional text with optional formatting to an issue's description.")]
    public async Task AppendIssueDescription([ActionParameter] IssueIdentifier issueIdentifier,
        [ActionParameter] AppendDescriptionRequest input)
    {
        var request = new JiraRequest($"/issue/{issueIdentifier.IssueKey}", Method.Get);
        var issue = await Client.ExecuteWithHandling<IssueWrapper>(request);

        var contentElement = new ContentElement() 
        {
            Type = "text",
            Text = input.Text
        };
        
        if(input.Formatting != null && input.Formatting != "none")
        {
            contentElement.Marks =
            [
                new Mark { Type = input.Formatting }
            ];
        }

        if (issue.Fields.Description is null) 
        {
            issue.Fields.Description = new Description { Type = "doc", Version = 1 ,Content = new List<ContentElement> { new ContentElement 
            {
                Type = "paragraph", 
                Content = new List<ContentElement> { contentElement }

            } } };
        }
        else 
        {
            issue.Fields.Description?.Content.Add(new ContentElement
            { Type = "paragraph", Content = new List<ContentElement> { contentElement } });
        }

        var body = new { fields = new { description = issue.Fields.Description } };
        var updateIssueRequest = new JiraRequest($"/issue/{issueIdentifier.IssueKey}", Method.Put)
            .WithJsonBody(body, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore
            });

        await Client.ExecuteWithHandling(updateIssueRequest);
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