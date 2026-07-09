using Apps.Jira.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers.CustomFields;

public class CustomCascadingParentOptionsDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    private readonly IssueIdentifier _issue;
    private readonly CustomCascadingFieldIdentifier _field;

    public CustomCascadingParentOptionsDataSourceHandler(InvocationContext invocationContext,
        [ActionParameter] IssueIdentifier issue,
        [ActionParameter] CustomCascadingFieldIdentifier field) : base(invocationContext)
    {
        _issue = issue;
        _field = field;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(_issue.IssueKey) || string.IsNullOrWhiteSpace(_field.CustomCascadingFieldId))
            return result;

        var request = new JiraRequest($"/issue/{_issue.IssueKey}/editmeta", Method.Get);
        var editMeta = await Client.ExecuteWithHandling<JObject>(request);

        var allowedValues = editMeta["fields"]?[_field.CustomCascadingFieldId]?["allowedValues"] as JArray;
        if (allowedValues == null || allowedValues.Count == 0)
            return result;

        foreach (var option in allowedValues)
        {
            var id = option["id"]?.ToString();
            var value = option["value"]?.ToString();

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(value))
                continue;

            if (!string.IsNullOrWhiteSpace(context.SearchString) &&
                !value.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                continue;

            result[id] = value;
        }

        return result;
    }
}
