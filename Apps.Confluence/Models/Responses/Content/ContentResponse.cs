using Apps.Confluence.Models.Responses.Spaces;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Confluence.Models.Responses.Content;

public class ContentResponse
{
    [Display("Content ID")]
    public string Id { get; set; } = string.Empty;
    
    [Display("Content type")]
    public string Type { get; set; } = string.Empty;
    
    [Display("Status")]
    public string Status { get; set; } = string.Empty;
    
    [Display("Title")]
    public string Title { get; set; } = string.Empty;

    [Display("Body")] 
    public BodyResponse Body { get; set; } = new();
    
    [DefinitionIgnore]
    public VersionResponse Version { get; set; } = new();
    
    [DefinitionIgnore] 
    public HistoryResponse? History { get; set; }

    [Display("Space")]
    public SpaceResponse Space { get; set; } = new();

    public List<Ancestor>? Ancestors { get; set; }

    public string? ParentId => Ancestors?.LastOrDefault()?.Id;
}

public class BodyResponse
{
    [Display("View")]
    public ViewResponse View { get; set; } = new();

    [DefinitionIgnore]
    [Display("Storage")]
    public ViewResponse Storage { get; set; } = new();
}

public class Ancestor
{
    public string Id { get; set; }
}
public class ViewResponse
{
    [Display("HTML")]
    public string Value { get; set; } = string.Empty;
    
    [Display("Representation")]
    public string Representation { get; set; } = string.Empty;
}

public class VersionResponse
{
    [Display("Number")]
    public int Number { get; set; }
}

public class HistoryResponse
{
    public DateTime CreatedDate { get; set; }

    public LastUpdatedResponse LastUpdated { get; set; } = new();
}

public class LastUpdatedResponse
{
    public DateTime When { get; set; }
}