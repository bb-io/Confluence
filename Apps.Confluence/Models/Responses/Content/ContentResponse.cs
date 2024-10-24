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
}