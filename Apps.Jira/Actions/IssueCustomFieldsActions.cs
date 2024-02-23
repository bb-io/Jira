using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
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

    [Action("Get custom string or dropdown field value",
            Description = "Retrieve the value of a custom string or dropdown field for a specific issue.")]
        public async Task<GetCustomFieldValueResponse<string>> GetCustomStringFieldValue(
            [ActionParameter] IssueIdentifier issue, [ActionParameter] CustomStringFieldIdentifier customStringField)
        {
            var targetField = await GetCustomFieldData(customStringField.CustomStringFieldId);
            var getIssueResponse = await GetIssue(issue.IssueKey);
            var requestedField =
                JObject.Parse(getIssueResponse.Content)["fields"][customStringField.CustomStringFieldId];

            string requestedFieldValue;

            if (targetField.Schema!.Type == "string")
                requestedFieldValue = requestedField.ToString();
            else
                requestedFieldValue = requestedField["value"].ToString();
            
            return new GetCustomFieldValueResponse<string> { Value = requestedFieldValue };
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

    #endregion

    #region Put

    [Action("Set custom string or dropdown field value", 
            Description = "Set the value of a custom string or dropdown field for a specific issue.")]
        public async Task SetCustomStringFieldValue([ActionParameter] IssueIdentifier issue, 
            [ActionParameter] CustomStringFieldIdentifier customStringField,
            [ActionParameter] [Display("Value")] string value)
        {
            var targetField = await GetCustomFieldData(customStringField.CustomStringFieldId);
            string requestBody;
            
            if (targetField.Schema!.Type == "string")
                requestBody = $@"
                {{
                    ""fields"": {{
                        ""{customStringField.CustomStringFieldId}"": ""{value}""
                    }}
                }}";
            else
                requestBody = $@"
                {{
                    ""fields"": {{
                        ""{customStringField.CustomStringFieldId}"": {{
                            ""value"": ""{value}""
                        }}
                    }}
                }}";
            
            var updateFieldRequest = new JiraRequest($"/issue/{issue.IssueKey}", Method.Put);
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
            
            var updateFieldRequest = new JiraRequest($"/issue/{issue.IssueKey}", Method.Put);
            updateFieldRequest.AddJsonBody($@"
                {{
                    ""fields"": {{
                        ""{customDateField.CustomDateFieldId}"": ""{dateString}""
                    }}
                }}");
            
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

    #endregion
}