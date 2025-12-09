using Apps.Jira.Models.Identifiers;
using Apps.Jira.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers.CustomFields
{
    public class CustomMulticheckboxesOptionsDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
    {
        private readonly IssueIdentifier _issue;
        private readonly CustomMulticheckboxesFieldIdentifier _field;

        public CustomMulticheckboxesOptionsDataSourceHandler(InvocationContext invocationContext,
            [ActionParameter] IssueIdentifier issue, [ActionParameter] CustomMulticheckboxesFieldIdentifier field)
             : base(invocationContext) { _issue = issue; _field = field; }

        public async Task<Dictionary<string, string>> GetDataAsync(
            DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var result = new Dictionary<string, string>();

            var fieldId = _field.CustomMulticheckboxesFieldId;
            if (string.IsNullOrWhiteSpace(fieldId))
                return result;

            var request = new JiraRequest($"/issue/{_issue.IssueKey}/editmeta", Method.Get);
            var editMeta = await Client.ExecuteWithHandling<JObject>(request);

            var fieldToken = editMeta["fields"]?[fieldId];
            if (fieldToken == null)
                return result;

            var allowedValues = fieldToken["allowedValues"] as JArray;
            if (allowedValues == null || allowedValues.Count == 0)
                return result;

            foreach (var option in allowedValues)
            {
                var value = option["value"]?.ToString();
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                if (!string.IsNullOrEmpty(context.SearchString) &&
                    !value.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                    continue;

                result[value] = value;
            }

            return result;
        }

        private class JiraFieldOption
        {
            [JsonProperty("id")]
            public string Id { get; set; } = default!;

            [JsonProperty("value")]
            public string Value { get; set; } = default!;
        }
    }
}
