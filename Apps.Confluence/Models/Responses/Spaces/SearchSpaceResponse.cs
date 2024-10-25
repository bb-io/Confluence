using Blackbird.Applications.Sdk.Common;

namespace Apps.Confluence.Models.Responses.Spaces;

public class SearchSpaceResponse : BasePaginationResponse<SpaceResponse>
{
    [Display("Spaces")]
    public override List<SpaceResponse> Results { get; set; } = new();
}