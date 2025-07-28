using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Confluence.Models.Responses.Content;

public class SearchContentResponse : BasePaginationResponse<ContentResponse>
{
    [Display("Contents")]
    public override List<ContentResponse> Results { get; set; } = new();

    [JsonProperty("_links")]
    public SearchResponseLinks Links { get; set; } = new();
}

public class SearchResponseLinks
{
    [JsonProperty("next")]
    public string Next { get; set; } = string.Empty;
}