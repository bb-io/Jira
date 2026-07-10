using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers.CustomFields;

internal static class CustomCascadingOptionsLookup
{
    public static async Task<List<CustomFieldOptionDto>> GetAllOptionsAsync(JiraClient client, string projectKey,
        string issueTypeId, string fieldId)
    {
        var request = new JiraRequest("/issue/createmeta", Method.Get)
            .AddQueryParameter("projectKeys", projectKey)
            .AddQueryParameter("issuetypeIds", issueTypeId)
            .AddQueryParameter("expand", "projects.issuetypes.fields");
        var response = await client.ExecuteWithHandling<JObject>(request);

        var allowedValues = response["projects"]?.First?["issuetypes"]?.First?["fields"]?[fieldId]?["allowedValues"] as JArray;
        if (allowedValues == null || allowedValues.Count == 0)
            return [];

        var result = new List<CustomFieldOptionDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var option in ParseAllowedValues(allowedValues))
        {
            if (string.IsNullOrWhiteSpace(option.Id) || !seen.Add(option.Id))
                continue;

            result.Add(option);
        }

        return result;
    }

    private static List<CustomFieldOptionDto> ParseAllowedValues(JArray allowedValues)
    {
        var options = new List<CustomFieldOptionDto>();

        foreach (var option in allowedValues.OfType<JObject>())
        {
            ParseOption(option, null, options);
        }

        return options;
    }

    private static void ParseOption(JObject optionToken, string? parentOptionId, List<CustomFieldOptionDto> result)
    {
        var id = optionToken["id"]?.ToString();
        var value = optionToken["value"]?.ToString() ?? optionToken["name"]?.ToString();
        var resolvedParentId = optionToken["optionId"]?.ToString() ?? optionToken["parentId"]?.ToString() ?? parentOptionId;

        if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(value))
        {
            result.Add(new CustomFieldOptionDto
            {
                Id = id,
                Value = value,
                OptionId = resolvedParentId
            });
        }

        foreach (var childArray in GetChildOptionArrays(optionToken))
        {
            foreach (var child in childArray.OfType<JObject>())
            {
                ParseOption(child, id, result);
            }
        }
    }

    private static IEnumerable<JArray> GetChildOptionArrays(JObject optionToken)
    {
        foreach (var property in optionToken.Properties())
        {
            if (property.Value is not JArray array || array.Count == 0)
                continue;

            if (array.All(item => item is JObject child && HasOptionIdentity(child)))
                yield return array;
        }
    }

    private static bool HasOptionIdentity(JObject token)
    {
        return token["id"] != null && (token["value"] != null || token["name"] != null);
    }

    internal class CustomFieldOptionDto
    {
        public string Id { get; set; } = default!;

        public string Value { get; set; } = default!;

        public string? OptionId { get; set; }
    }
}
