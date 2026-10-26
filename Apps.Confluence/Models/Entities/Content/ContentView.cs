using Blackbird.Applications.Sdk.Common;

namespace Apps.Confluence.Models.Entities.Content;

public class ContentView
{
    [Display("HTML")]
    public string Value { get; set; } = string.Empty;
    
    [Display("Representation")]
    public string Representation { get; set; } = string.Empty;
}