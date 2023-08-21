using Newtonsoft.Json;

namespace Apps.Jira.Extensions;

public static class SerializationExtensions
{
    public static T Deserialize<T>(this string content)
    {
        var deserializedContent = JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            }
        );
        return deserializedContent;
    }
}