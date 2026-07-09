using Apps.Jira.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers.CustomFields;

public class CustomCascadingFieldDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    private readonly IssueIdentifier _issue;

    public CustomCascadingFieldDataSourceHandler(InvocationContext invocationContext, [ActionParameter] IssueIdentifier issue)
        : base(invocationContext)
    {
        _issue = issue;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(_issue.IssueKey))
            return result;

        var request = new JiraRequest($"/issue/{_issue.IssueKey}/editmeta", Method.Get);
        var editMeta = await Client.ExecuteWithHandling<JObject>(request);
        var fields = editMeta["fields"] as JObject;
        if (fields == null)
            return result;

        foreach (var property in fields.Properties())
        {
            if (!property.Name.StartsWith("customfield_", StringComparison.OrdinalIgnoreCase))
                continue;

            if (property.Value is not JObject field)
                continue;

            var name = field["name"]?.ToString();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var allowedValues = field["allowedValues"] as JArray;
            var hasChildren = allowedValues?.Any(x => x?["children"] is JArray children && children.Count > 0) == true;
            if (!hasChildren)
                continue;

            if (!string.IsNullOrWhiteSpace(context.SearchString) &&
                !name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                continue;

            result[property.Name] = name;
        }

        return result;
    }
}
