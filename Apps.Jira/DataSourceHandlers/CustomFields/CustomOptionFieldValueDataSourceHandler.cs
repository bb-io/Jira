using System.ComponentModel.Design;
using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers.CustomFields;

public class CustomOptionFieldValueDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    private record FieldValue(string Value, string DisplayName);
    private record FieldValuesWrapper(IEnumerable<FieldValue> Results);
    
    private readonly CustomOptionFieldIdentifier _customOptionField;

    public CustomOptionFieldValueDataSourceHandler(InvocationContext invocationContext,
        [ActionParameter] CustomOptionFieldIdentifier customOptionField) : base(invocationContext)
    {
        _customOptionField = customOptionField;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (_customOptionField.CustomOptionFieldId == null)
            throw new Exception("Please specify custom dropdown field ID first.");
        
        var getFieldsRequest = new JiraRequest("/field", Method.Get);
        var fields = await Client.ExecuteWithHandling<IEnumerable<FieldDto>>(getFieldsRequest);
        var targetField = fields.First(field => field.Id == _customOptionField.CustomOptionFieldId);

        var getPossibleValuesRequest =
            new JiraRequest($"/jql/autocompletedata/suggestions?fieldName={targetField.Name}", Method.Get);
        var fieldValues = await Client.ExecuteWithHandling<FieldValuesWrapper>(getPossibleValuesRequest);

        return fieldValues.Results
            .Where(value => context.SearchString == null ||
                            value.DisplayName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(value => value.DisplayName.Replace("&quot;", "\""))
            .ToDictionary(value => value, value => value);
    }
}