using Apps.Confluence.Models.Entities.Space;
using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Confluence.Models.Entities.Content;

public class ContentEntity
{
    [Display("Content ID"), JsonProperty("id")]
    public string ContentId { get; set; } = string.Empty;
    
    [Display("Content type")]
    public string Type { get; set; } = string.Empty;
    
    [Display("Status")]
    public string Status { get; set; } = string.Empty;
    
    [Display("Title")]
    public string Title { get; set; } = string.Empty;

    [Display("Body")] 
    public ContentBody Body { get; set; } = new();
    
    [DefinitionIgnore]
    public ContentVersion Version { get; set; } = new();
    
    [DefinitionIgnore] 
    public ContentHistory? History { get; set; }

    public SpaceEntity Space { get; set; } = new();

    public List<ContentAncestor>? Ancestors { get; set; }

    public string? ParentId => Ancestors?.LastOrDefault()?.Id;
}