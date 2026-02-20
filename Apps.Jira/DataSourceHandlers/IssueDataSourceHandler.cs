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

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
       CancellationToken cancellationToken)
    {
        var projectKeys = await GetProjectKeysAsync(50, cancellationToken);

        var safeProjectKeys = projectKeys
            .Select(NormalizeKey)
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Where(IsValidProjectKey)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(50)
            .Select(QuoteJqlValue)
            .ToList();

        var boundedScope = safeProjectKeys.Any()
            ? $"project in ({string.Join(", ", safeProjectKeys)})"
            : "updated >= -180d";

        var search = NormalizeSearch(context.SearchString);

        string jql = !string.IsNullOrWhiteSpace(search)
            ? $"({boundedScope}) AND (summary ~ \"{EscapeForJql(search)}\" OR description ~ \"{EscapeForJql(search)}\") ORDER BY updated DESC"
            : $"{boundedScope} ORDER BY updated DESC";

        IssuesWrapper response;
        try
        {
            response = await SearchIssues(jql, cancellationToken);
        }
        catch (Exception ex)
        {
            InvocationContext.Logger?.LogWarning(
                $"[IssueDataSource] Primary JQL failed. Falling back. Error: {ex.Message}. JQL: {jql}", null);

            var fallbackJql = !string.IsNullOrWhiteSpace(search)
                ? $"updated >= -180d AND (summary ~ \"{EscapeForJql(search)}\" OR description ~ \"{EscapeForJql(search)}\") ORDER BY updated DESC"
                : "updated >= -180d ORDER BY updated DESC";

            response = await SearchIssues(fallbackJql, cancellationToken);
        }

        return response.Issues.ToDictionary(
            i => i.Key,
            i => $"{i.Fields.Summary} ({i.Fields.Project.Name} project)");
    }

    private async Task<IssuesWrapper> SearchIssues(string jql, CancellationToken cancellationToken)
    {
        InvocationContext.Logger?.LogInformation($"[IssueDataSource] JQL: {jql}", null);

        var request = new JiraRequest("/search/jql", Method.Get);
        request.AddQueryParameter("maxResults", "20");
        request.AddQueryParameter("fields", "summary,project");
        request.AddQueryParameter("fieldsByKeys", "true");
        request.AddQueryParameter("jql", jql);

        return await Client.ExecuteWithHandling<IssuesWrapper>(request);
    }

    private async Task<List<string>> GetProjectKeysAsync(int limit, CancellationToken ct)
    {
        var req = new JiraRequest("/project/search", Method.Get);
        req.AddQueryParameter("maxResults", limit.ToString());
        req.AddQueryParameter("orderBy", "lastIssueUpdatedTime");

        var resp = await Client.ExecuteWithHandling<ProjectSearchResponse>(req);

        return resp.Values?
                   .Select(v => v.Key)
                   .Where(k => !string.IsNullOrWhiteSpace(k))
                   .Distinct()
                   .Take(limit)
                   .ToList()
               ?? new List<string>();
    }

    private static string NormalizeKey(string k)
    {
        k = (k ?? string.Empty).Trim();

        return k.Replace("\u200B", "")
                .Replace("\u200C", "")
                .Replace("\u200D", "")
                .Replace("\uFEFF", "");
    }

    private static bool IsValidProjectKey(string k)
    {
        return Regex.IsMatch(k, @"^[A-Za-z][A-Za-z0-9]{1,9}$");
    }

    private static string QuoteJqlValue(string value)
    {
        value = (value ?? string.Empty).Replace(@"\", @"\\").Replace("\"", "\\\"");
        return $"\"{value}\"";
    }

    private static string NormalizeSearch(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        input = Regex.Replace(input, @"\s+", " ");
        return input.Trim();
    }

    private static string EscapeForJql(string input)
    {
        return (input ?? string.Empty).Replace(@"\", @"\\").Replace("\"", "\\\"");
    }
}