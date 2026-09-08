using Apps.Confluence.Utils;
using Newtonsoft.Json;

namespace Apps.Confluence.Models.Responses.Properties;

public class ContentPropertyResponse
{
    public string Id { get; set; }   
    public string Key { get; set; }

    [JsonConverter(typeof(RawJsonStringConverter))]
    public string Value { get; set; }

    public PropertyVersion Version { get; set; }
}

public class PropertyVersion
{
    public int Number { get; set; }
}
