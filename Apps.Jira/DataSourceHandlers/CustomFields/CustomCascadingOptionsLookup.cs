using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers.CustomFields;

internal static class CustomCascadingOptionsLookup
{
    public static async Task<List<CustomFieldContextOptionDto>> GetAllOptionsAsync(JiraClient client, string fieldId)
    {
        var contexts = await GetContextsAsync(client, fieldId);
        var result = new List<CustomFieldContextOptionDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var context in contexts)
        {
            var optionsRequest = new JiraRequest($"/field/{fieldId}/context/{context.Id}/option", Method.Get);
            var options = await client.Paginate<CustomFieldContextOptionDto>(optionsRequest);

            foreach (var option in options)
            {
                if (string.IsNullOrWhiteSpace(option.Id) || !seen.Add(option.Id))
                    continue;

                result.Add(option);
            }
        }

        return result;
    }

    private static async Task<List<CustomFieldContextDto>> GetContextsAsync(JiraClient client, string fieldId)
    {
        var contextsRequest = new JiraRequest($"/field/{fieldId}/context", Method.Get);
        return await client.Paginate<CustomFieldContextDto>(contextsRequest);
    }

    internal class CustomFieldContextDto
    {
        public string Id { get; set; } = default!;
    }

    internal class CustomFieldContextOptionDto
    {
        public string Id { get; set; } = default!;

        public string Value { get; set; } = default!;

        public string? OptionId { get; set; }
    }
}
