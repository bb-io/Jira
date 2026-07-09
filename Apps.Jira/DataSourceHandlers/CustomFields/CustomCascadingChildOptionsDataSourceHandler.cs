using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers.CustomFields;

public class CustomCascadingChildOptionsDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    private readonly IssueIdentifier _issue;
    private readonly CustomCascadingFieldIdentifier _field;
    private readonly CustomCascadingFieldValueInput _input;

    public CustomCascadingChildOptionsDataSourceHandler(InvocationContext invocationContext,
        [ActionParameter] IssueIdentifier issue,
        [ActionParameter] CustomCascadingFieldIdentifier field,
        [ActionParameter] CustomCascadingFieldValueInput input) : base(invocationContext)
    {
        _issue = issue;
        _field = field;
        _input = input;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(_issue.IssueKey) ||
            string.IsNullOrWhiteSpace(_field.CustomCascadingFieldId) ||
            string.IsNullOrWhiteSpace(_input.ParentOptionId))
            return result;

        var request = new JiraRequest($"/issue/{_issue.IssueKey}/editmeta", Method.Get);
        var editMeta = await Client.ExecuteWithHandling<JObject>(request);

        var allowedValues = editMeta["fields"]?[_field.CustomCascadingFieldId]?["allowedValues"] as JArray;
        if (allowedValues == null || allowedValues.Count == 0)
            return result;

        var parentOption = allowedValues.FirstOrDefault(option =>
            string.Equals(option?["id"]?.ToString(), _input.ParentOptionId, StringComparison.OrdinalIgnoreCase));

        var childOptions = parentOption?["children"] as JArray;
        if (childOptions == null || childOptions.Count == 0)
            return result;

        foreach (var child in childOptions)
        {
            var id = child["id"]?.ToString();
            var value = child["value"]?.ToString();

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
