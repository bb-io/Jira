using Apps.Jira.DataSourceHandlers.CustomFields;
using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
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
        try 
        {
            var requestedField = JObject.Parse(getIssueResponse.Content)["fields"][customStringField.CustomStringFieldId]
            .ToString();

            return new GetCustomFieldValueResponse<string> { Value = requestedField };
        } 
        catch 
        {
            return new GetCustomFieldValueResponse<string> ();
        }

    }


    [Action("Get custom number field value",
        Description = "Retrieve the value of a custom number field for a specific issue.")]
    public async Task<GetCustomFieldValueResponse<string>> GetCustomNumericFieldValue(
        [ActionParameter] IssueIdentifier issue, [ActionParameter] CustomNumericFieldIdentifier customStringField)
    {
        var getIssueResponse = await GetIssue(issue.IssueKey);
        try
        {
            var requestedField = JObject.Parse(getIssueResponse.Content)["fields"][customStringField.CustomNumberFieldId]
            .ToString();

            return new GetCustomFieldValueResponse<string> { Value = requestedField };
        }
        catch
        {
            return new GetCustomFieldValueResponse<string>();
        }

    }


    [Action("Get custom dropdown field value",
        Description = "Retrieve the value of a custom dropdown field for a specific issue.")]
    public async Task<GetCustomFieldValueResponse<string>> GetCustomOptionFieldValue(
        [ActionParameter] IssueIdentifier issue, [ActionParameter] CustomOptionFieldIdentifier customOptionField)
    {
        var getIssueResponse = await GetIssue(issue.IssueKey);
        try 
        {
           var requestedField = JObject.Parse(getIssueResponse.Content)["fields"][customOptionField.CustomOptionFieldId]["value"]
                .ToString();
            return new GetCustomFieldValueResponse<string> { Value = requestedField };
        } 
        catch 
        {
            return new GetCustomFieldValueResponse<string>();
        }     
    }

    [Action("Get custom date field value",
        Description = "Retrieve the value of a custom date field for a specific issue.")]
    public async Task<GetCustomFieldValueResponse<DateTime>> GetCustomDateFieldValue(
        [ActionParameter] IssueIdentifier issue, [ActionParameter] CustomDateFieldIdentifier customStringField)
    {
        var getIssueResponse = await GetIssue(issue.IssueKey);
        try 
        {
            var requestedFieldValue =
            JObject.Parse(getIssueResponse.Content)["fields"][customStringField.CustomDateFieldId]
                .ToString();
            if (String.IsNullOrEmpty(requestedFieldValue)) { return new GetCustomFieldValueResponse<DateTime>(); }
            return new GetCustomFieldValueResponse<DateTime> { Value = DateTime.Parse(requestedFieldValue) };

        }
        catch 
        {
            return new GetCustomFieldValueResponse<DateTime>();
        }
    }

    [Action("Get custom multiselect field values",
    Description = "Retrieve the values of a custom multiselect field for a specific issue.")]
    public async Task<List<string>> GetCustomMultiselectFieldValue(
    [ActionParameter] IssueIdentifier issue, [ActionParameter] CustomMultiselectFieldIdentifier customMultiselectField)
    {
        var getIssueResponse = await GetIssue(issue.IssueKey);
        JObject Parsedissue = JObject.Parse(getIssueResponse.Content);
        var customField = Parsedissue["fields"][customMultiselectField.CustomMultiselectFieldId];

        List<string> values = new List<string>();

        if (customField != null)
        {
            if (customField.Type == JTokenType.Array)
            {
                foreach (var item in customField)
                {
                    values.Add(item["value"].ToString());
                }
            }
            else if (customField.Type == JTokenType.Object)
            {
                values.Add(customField["value"].ToString());
            }
            else if (customField.Type == JTokenType.String || customField.Type == JTokenType.Integer ||
                     customField.Type == JTokenType.Float || customField.Type == JTokenType.Boolean ||
                     customField.Type == JTokenType.Date)
            {
                values.Add(customField.ToString());
            }
            else 
            {
                values.Add(customField.ToString());
            }
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

    [Action("Set custom multiselect field value",
        Description = "Set the values of a custom multiselect field for a specific issue.")]
    public async Task SetCustomMultiselectFieldValue([ActionParameter] IssueIdentifier issue,
        [ActionParameter] CustomMultiselectFieldIdentifier customStringField,
        [ActionParameter] CustomMultiselectFieldInput values)
    {
        var multiSelectValues = values.ValueProperty.Select(v => new { value = v }).ToList();

        var requestBody = new
        {
            fields = new Dictionary<string, object>
        {
            { customStringField.CustomMultiselectFieldId, multiSelectValues }
        }
        };

        await SetCustomFieldValue(requestBody, issue.IssueKey);
    }


    [Action("Set custom number field value",
        Description = "Set the value of a custom string field for a specific issue.")]
    public async Task SetCustomNumericFieldValue([ActionParameter] IssueIdentifier issue,
        [ActionParameter] CustomNumericFieldIdentifier customStringField,
        [ActionParameter][Display("Value")] double value)
    {
        var requestBody = new
        {
            fields = new Dictionary<string, double> { { customStringField.CustomNumberFieldId, value } }
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

    [Action("Set custom rich text field value", Description = "Set the value of a custom rich text field for a specific issue.")]
    public async Task SetCustomRichTextFieldValue(
    [ActionParameter] IssueIdentifier issue,
    [ActionParameter] CustomStringFieldIdentifier customTextField,
    [ActionParameter][Display("Text")] string text,
    //[ActionParameter] 
    RichTextMarksRequest marks = null)
    {
        var targetField = await GetCustomFieldData(customTextField.CustomStringFieldId);

        var markNodes = marks != null && marks?.Marks?.Any() == true ? marks.Marks.Select(mark => new Dictionary<string, object>
        {
            { "type", mark }
        }).ToList()
    : null;

        var document = new Dictionary<string, object>
    {
        { "version", 1 },
        { "type", "doc" },
        { "content", new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "type", "paragraph" },
                    { "content", new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object>
                            {
                                { "type", "text" },
                                { "text", text },
                                { "marks", markNodes ?? new List<Dictionary<string, object>>() }
                            }
                        }
                    }
                }
            }
        }
    };

        var jsonBody = new Dictionary<string, object>
    {
        { "fields", new Dictionary<string, object>
            {
                { customTextField.CustomStringFieldId, document }
            }
        }
    };

        var requestBody = JsonConvert.SerializeObject(jsonBody);
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
            throw new PluginApplicationException("Couldn't set field value. Please make sure that field exists for specific issue " +
                                "type in the project.");
        }
    }

    #endregion
}