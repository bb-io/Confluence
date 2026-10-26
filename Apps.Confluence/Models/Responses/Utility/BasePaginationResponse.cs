using Newtonsoft.Json;

namespace Apps.Confluence.Models.Responses.Utility;

public class BasePaginationResponse<T>
{
    public virtual List<T> Results { get; set; } = new();

    [JsonProperty("start")]
    public int Start { get; set; }

    [JsonProperty("limit")]
    public int Limit { get; set; }
    
    [JsonProperty("size")]
    public int? Size { get; set; }
    
    [JsonProperty("_links")]
    public ResponseLinks? Links { get; set; } = new();
}