using Apps.Confluence.DataSourceHandlers;
using Apps.Confluence.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Confluence.Models.Requests.Content;

public class CreateContentRequest
{
    [Display("Content type"), StaticDataSource(typeof(ContentTypeDataSource))]
    public string Type { get; set; } = string.Empty;
    
    [Display("Title")]
    public string Title { get; set; } = string.Empty;
    
    [Display("Space ID"), DataSource(typeof(SpaceDataSource))]
    public string SpaceId { get; set; } = string.Empty;

    [StaticDataSource(typeof(ContentStatusDataSource))]
    public string? Status { get; set; }
    
    [Display("Body HTML")]
    public string? Body { get; set; }
}