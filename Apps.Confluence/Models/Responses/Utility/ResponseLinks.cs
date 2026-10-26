using Newtonsoft.Json;

namespace Apps.Confluence.Models.Responses.Utility;

public class ResponseLinks
{
    [JsonProperty("next")]
    public string Next { get; set; } = string.Empty;
}