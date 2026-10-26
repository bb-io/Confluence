using Blackbird.Applications.Sdk.Common;

namespace Apps.Confluence.Models.Entities.Content;

public class ContentBody
{
    [Display("View")]
    public ContentView View { get; set; } = new();

    [DefinitionIgnore]
    [Display("Storage")]
    public ContentView Storage { get; set; } = new();
}