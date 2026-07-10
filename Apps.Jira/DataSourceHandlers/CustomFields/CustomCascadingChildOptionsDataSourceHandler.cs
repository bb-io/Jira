using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Jira.DataSourceHandlers.CustomFields;

public class CustomCascadingChildOptionsDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    private readonly ProjectIdentifier _project;
    private readonly IssueTypeIdentifier _issueType;
    private readonly CustomCascadingFieldIdentifier _field;
    private readonly CustomCascadingFieldValueInput _input;

    public CustomCascadingChildOptionsDataSourceHandler(InvocationContext invocationContext,
        [ActionParameter] ProjectIdentifier project,
        [ActionParameter] IssueTypeIdentifier issueType,
        [ActionParameter] CustomCascadingFieldIdentifier field,
        [ActionParameter] CustomCascadingFieldValueInput input) : base(invocationContext)
    {
        _project = project;
        _issueType = issueType;
        _field = field;
        _input = input;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(_project.ProjectKey) ||
            string.IsNullOrWhiteSpace(_issueType.IssueTypeId) ||
            string.IsNullOrWhiteSpace(_field.CustomCascadingFieldId) ||
            string.IsNullOrWhiteSpace(_input.ParentOptionId))
            return result;

        var options = await CustomCascadingOptionsLookup.GetAllOptionsAsync(Client, _project.ProjectKey,
            _issueType.IssueTypeId, _field.CustomCascadingFieldId);
        foreach (var child in options.Where(x =>
                     string.Equals(x.OptionId, _input.ParentOptionId, StringComparison.OrdinalIgnoreCase)))
        {
            var id = child.Id;
            var value = child.Value;

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
