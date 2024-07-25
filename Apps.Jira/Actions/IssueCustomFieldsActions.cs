using Apps.Jira.DataSourceHandlers.CustomFields;
using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.Jira.Actions;

[ActionList]
public class IssueCustomFieldsActions : JiraInvocable
{
    public IssueCustomFieldsActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    #region Get

    [Action("Get custom text field value",
        Description = "Retrieve the value of a custom string field for a specific issue.")]
    public async Task<GetCustomFieldValueResponse<string>> GetCustomStringFieldValue(
        [ActionParameter] IssueIdentifier issue, [ActionParameter] CustomStringFieldIdentifier customStringField)
    {
        var getIssueResponse = await GetIssue(issue.IssueKey);
        var requestedField = JObject.Parse(getIssueResponse.Content)["fields"][customStringField.CustomStringFieldId]
            .ToString();

        return new GetCustomFieldValueResponse<string> { Value = requestedField };
    }
    
    [Action("Get custom dropdown field value",
        Description = "Retrieve the value of a custom dropdown field for a specific issue.")]
    public async Task<GetCustomFieldValueResponse<string>> GetCustomOptionFieldValue(
        [ActionParameter] IssueIdentifier issue, [ActionParameter] CustomOptionFieldIdentifier customOptionField)
    {
        var getIssueResponse = await GetIssue(issue.IssueKey);
        var requestedField =
            JObject.Parse(getIssueResponse.Content)["fields"][customOptionField.CustomOptionFieldId]["value"]
                .ToString();

        return new GetCustomFieldValueResponse<string> { Value = requestedField };
    }

    [Action("Get custom date field value",
        Description = "Retrieve the value of a custom date field for a specific issue.")]
    public async Task<GetCustomFieldValueResponse<DateTime>> GetCustomDateFieldValue(
        [ActionParameter] IssueIdentifier issue, [ActionParameter] CustomDateFieldIdentifier customStringField)
    {
        var getIssueResponse = await GetIssue(issue.IssueKey);
        var requestedFieldValue =
            DateTime.Parse(JObject.Parse(getIssueResponse.Content)["fields"][customStringField.CustomDateFieldId]
                .ToString());

        return new GetCustomFieldValueResponse<DateTime> { Value = requestedFieldValue };
    }

    [Action("Get custom multiselect field value",
    Description = "Retrieve the values of a custom multiselect field for a specific issue.")]
    public async Task<List<string>> GetCustomMultiselectFieldValue(
    [ActionParameter] IssueIdentifier issue, [ActionParameter] CustomMultiselectFieldIdentifier customMultiselectField)
    {
        var getIssueResponse = await GetIssue(issue.IssueKey);
        JObject Parsedissue = JObject.Parse(getIssueResponse.Content);
        JArray customFieldArray = (JArray)Parsedissue["fields"]["customfield_10035"];

        List<string> values = new List<string>();
        foreach (var item in customFieldArray)
        {
            values.Add(item["value"].ToString());
        }
        return values;
    }

    #endregion

    #region Put

    [Action("Set custom text field value",
        Description = "Set the value of a custom string field for a specific issue.")]
    public async Task SetCustomStringFieldValue([ActionParameter] IssueIdentifier issue,
        [ActionParameter] CustomStringFieldIdentifier customStringField,
        [ActionParameter] [Display("Value")] string value)
    {
        var requestBody = new
        {
            fields = new Dictionary<string, string> { { customStringField.CustomStringFieldId, value } }
        };

        await SetCustomFieldValue(requestBody, issue.IssueKey);
    }

    [Action("Set custom dropdown field value",
        Description = "Set the value of a custom dropdown field for a specific issue.")]
    public async Task SetCustomOptionFieldValue([ActionParameter] IssueIdentifier issue,
        [ActionParameter] CustomOptionFieldIdentifier customOptionField,
        [ActionParameter] [Display("Value")] [DataSource(typeof(CustomOptionFieldValueDataSourceHandler))] 
        string value)
    {
        var requestBody = new
        {
            fields = new Dictionary<string, object> { { customOptionField.CustomOptionFieldId, new { value } } }
        };

        await SetCustomFieldValue(requestBody, issue.IssueKey);
    }

    [Action("Set custom date field value",
        Description = "Set the value of a custom date field for a specific issue.")]
    public async Task SetCustomDateFieldValue([ActionParameter] IssueIdentifier issue,
        [ActionParameter] CustomDateFieldIdentifier customDateField,
        [ActionParameter] [Display("Value")] DateTime value)
    {
        var targetField = await GetCustomFieldData(customDateField.CustomDateFieldId);
        var dateString = targetField.Schema!.Type == "date"
            ? value.ToString("yyyy-MM-dd")
            : value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

        var requestBody = $@"
                {{
                    ""fields"": {{
                        ""{customDateField.CustomDateFieldId}"": ""{dateString}""
                    }}
                }}";

        await SetCustomFieldValue(requestBody, issue.IssueKey);
    }

    #endregion

    #region Utils

    private async Task<FieldDto> GetCustomFieldData(string customFieldId)
    {
        var getFieldsRequest = new JiraRequest("/field", Method.Get);
        var fields = await Client.ExecuteWithHandling<IEnumerable<FieldDto>>(getFieldsRequest);
        return fields.First(field => field.Id == customFieldId);
    }

    private async Task<RestResponse> GetIssue(string issueKey)
    {
        var request = new JiraRequest($"/issue/{issueKey}", Method.Get);
        return await Client.ExecuteWithHandling(request);
    }

    private async Task SetCustomFieldValue(object requestBody, string issueKey)
    {
        var updateFieldRequest = new JiraRequest($"/issue/{issueKey}", Method.Put);
        updateFieldRequest.AddJsonBody(requestBody);

        try
        {
            await Client.ExecuteWithHandling(updateFieldRequest);
        }
        catch
        {
            throw new Exception("Couldn't set field value. Please make sure that field exists for specific issue " +
                                "type in the project.");
        }
    }

    #endregion
}