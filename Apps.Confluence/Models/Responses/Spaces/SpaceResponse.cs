using Apps.Confluence.Models.Entities.Space;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Confluence.Models.Responses.Spaces;

public class SpaceResponse
{
    public SpaceResponse() { }
    
    public SpaceResponse(SpaceEntity entity)
    {
        Id = entity.Id;
        Key = entity.Key;
        Name = entity.Name;
        Type = entity.Type;
        Status = entity.Status;
    }

    [Display("Space ID")]
    public string Id { get; set; } = string.Empty;
    
    [Display("Space key")]
    public string Key { get; set; } = string.Empty;

    [Display("Space name")]
    public string Name { get; set; } = string.Empty;

    [Display("Space type")]
    public string Type { get; set; } = string.Empty;

    [Display("Space status")]
    public string Status { get; set; } = string.Empty;
}