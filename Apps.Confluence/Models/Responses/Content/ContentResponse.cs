using Apps.Confluence.Models.Entities.Content;
using Apps.Confluence.Models.Responses.Spaces;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Confluence.Models.Responses.Content;

public class ContentResponse(ContentEntity contentEntity) : IDownloadContentInput
{
    [Display("Content ID")]
    public string ContentId { get; set; } = contentEntity.ContentId;

    [Display("Content type")]
    public string Type { get; set; } = contentEntity.Type;

    [Display("Status")]
    public string Status { get; set; } = contentEntity.Status;

    [Display("Title")]
    public string Title { get; set; } = contentEntity.Title;

    [Display("Space")]
    public SpaceResponse Space { get; set; } = new(contentEntity.Space);
    
    [Display("Parent ID")]
    public string? ParentId { get; set; } = contentEntity.ParentId;

    [Display("Created at")]
    public DateTime? CreatedAt { get; set; } = contentEntity.History?.CreatedDate;

    [Display("Updated at")]
    public DateTime? UpdatedAt { get; set; } = contentEntity.History?.LastUpdated.When;
}