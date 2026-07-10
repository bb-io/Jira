using Apps.Jira.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Jira.DataSourceHandlers.CustomFields;

public class CustomCascadingParentOptionsDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    private readonly ProjectIdentifier _project;
    private readonly IssueTypeIdentifier _issueType;
    private readonly CustomCascadingFieldIdentifier _field;

    public CustomCascadingParentOptionsDataSourceHandler(InvocationContext invocationContext,
        [ActionParameter] ProjectIdentifier project,
        [ActionParameter] IssueTypeIdentifier issueType,
        [ActionParameter] CustomCascadingFieldIdentifier field) : base(invocationContext)
    {
        _project = project;
        _issueType = issueType;
        _field = field;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(_project.ProjectKey) ||
            string.IsNullOrWhiteSpace(_issueType.IssueTypeId) ||
            string.IsNullOrWhiteSpace(_field.CustomCascadingFieldId))
            return result;

        var options = await CustomCascadingOptionsLookup.GetAllOptionsAsync(Client, _project.ProjectKey,
            _issueType.IssueTypeId, _field.CustomCascadingFieldId);
        foreach (var option in options.Where(x => string.IsNullOrWhiteSpace(x.OptionId)))
        {
            var id = option.Id;
            var value = option.Value;

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
