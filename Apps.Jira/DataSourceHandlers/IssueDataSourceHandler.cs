using Apps.Jira.Dtos;
using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;
using System.Text.RegularExpressions;

namespace Apps.Jira.DataSourceHandlers;

public class IssueDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    public IssueDataSourceHandler(InvocationContext invocationContext): base(invocationContext) { }

    private const int DefaultWindowDays = 180;


    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {

        var projectKeys = await GetProjectKeysAsync(50, cancellationToken);

        var boundedScope = projectKeys.Any()
            ? $"project in ({string.Join(", ", projectKeys)})"
            : "updated >= -30d";

        string jql;
        if (!string.IsNullOrWhiteSpace(context.SearchString))
        {
            var term = EscapeForJql(context.SearchString);
            jql = $"({boundedScope}) AND (summary ~ \"{term}\" OR description ~ \"{term}\") ORDER BY updated DESC";
        }
        else
        {
            jql = $"{boundedScope} ORDER BY updated DESC";
        }

        var request = new JiraRequest("/search/jql", Method.Get);
        request.AddQueryParameter("maxResults", "20");
        request.AddQueryParameter("fields", "summary,project");
        request.AddQueryParameter("fieldsByKeys", "true");
        request.AddQueryParameter("jql", jql);

        var response = await Client.ExecuteWithHandling<IssuesWrapper>(request);

        return response.Issues.ToDictionary(
            i => i.Key,
            i => $"{i.Fields.Summary} ({i.Fields.Project.Name} project)");
    }

    private async Task<List<string>> GetProjectKeysAsync(int limit, CancellationToken ct)
    {
        var req = new JiraRequest("/project/search", Method.Get);
        req.AddQueryParameter("maxResults", limit.ToString());
        req.AddQueryParameter("orderBy", "lastIssueUpdatedTime");

        var resp = await Client.ExecuteWithHandling<ProjectSearchResponse>(req);
        return resp.Values?.Select(v => v.Key).Where(k => !string.IsNullOrWhiteSpace(k)).Distinct().Take(limit).ToList()
               ?? new List<string>();
    }

    private static string EscapeForJql(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return Regex.Replace(input, @"[\\\""]", m => "\\" + m.Value);
    }
}