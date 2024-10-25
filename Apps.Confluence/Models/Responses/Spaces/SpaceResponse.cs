using Blackbird.Applications.Sdk.Common;

namespace Apps.Confluence.Models.Responses.Spaces;

public class SpaceResponse
{
    [Display("Space ID")]
    public string Id { get; set; } = string.Empty;
    
    [Display("Space name")]
    public string Name { get; set; } = string.Empty;

    [Display("Space type")]
    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}