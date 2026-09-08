using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.Confluence.Utils;

public class RawJsonStringConverter : JsonConverter<string?>
{
    public override string? ReadJson(JsonReader reader, Type objectType, string? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        return token.Type switch
        {
            JTokenType.Null or JTokenType.Undefined => null,
            JTokenType.String => token.Value<string>(),
            _ => token.ToString(Formatting.None)
        };
    }

    public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer)
    {
        writer.WriteValue(value);
    }
}
