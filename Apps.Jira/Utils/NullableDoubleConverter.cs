using Newtonsoft.Json;

namespace Apps.Jira.Utils
{
    public class NullableDoubleConverter : JsonConverter<double>
    {
        public override double ReadJson(JsonReader reader, Type objectType, double existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            return Convert.ToDouble(reader.Value);
        }

        public override void WriteJson(JsonWriter writer, double value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}
