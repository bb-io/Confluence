using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Apps.Confluence.Constants;

public class JsonConfig
{
    public static JsonSerializerSettings JsonSettings => new()
    {
        ContractResolver = new DefaultContractResolver()
        {
            NamingStrategy = new DefaultNamingStrategy()
        },
        NullValueHandling = NullValueHandling.Ignore
    };
}