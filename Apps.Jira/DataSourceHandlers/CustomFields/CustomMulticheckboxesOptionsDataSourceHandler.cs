using Apps.Jira.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers.CustomFields
{
    public class CustomMulticheckboxesOptionsDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
    {
        private readonly CustomMulticheckboxesFieldIdentifier _input;
        public CustomMulticheckboxesOptionsDataSourceHandler(InvocationContext invocationContext, [ActionParameter] CustomMulticheckboxesFieldIdentifier input)
             : base(invocationContext) { _input = input; }

        public async Task<Dictionary<string, string>> GetDataAsync(
             DataSourceContext context,
             CancellationToken cancellationToken)
        {
            var result = new Dictionary<string, string>();

            var fieldId = _input.CustomMulticheckboxesFieldId;
            if (string.IsNullOrWhiteSpace(fieldId))
                return result;

            var startAt = 0;
            const int maxResults = 50;

            while (true)
            {
                var request = new JiraRequest($"/field/{fieldId}/option", Method.Get);
                request.AddQueryParameter("startAt", startAt.ToString());
                request.AddQueryParameter("maxResults", maxResults.ToString());

                var page = await Client.ExecuteWithHandling<JiraFieldOptionsResponse>(request);

                foreach (var o in page.Values)
                {
                    if (!string.IsNullOrEmpty(context.SearchString) &&
                        !o.Value.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                        continue;

                    result[o.Value] = o.Value;
                }

                if (page.IsLast || page.Values.Count == 0)
                    break;

                startAt += maxResults;
            }

            return result;
        }

        private class JiraFieldOptionsResponse
        {
            [JsonProperty("values")]
            public List<JiraFieldOption> Values { get; set; } = new();

            [JsonProperty("isLast")]
            public bool IsLast { get; set; }
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
