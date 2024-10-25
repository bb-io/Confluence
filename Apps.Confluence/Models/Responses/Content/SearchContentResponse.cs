using Blackbird.Applications.Sdk.Common;

namespace Apps.Confluence.Models.Responses.Content;

public class SearchContentResponse : BasePaginationResponse<ContentResponse>
{
    [Display("Contents")]
    public override List<ContentResponse> Results { get; set; } = new();
}