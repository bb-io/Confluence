using Apps.Confluence.DataSourceHandlers;
using Apps.Confluence.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Confluence.Models.Requests.Content;

public class FilterContentRequest
{
    [Display("Content type"), StaticDataSource(typeof(ContentTypeDataSource))]
    public string? ContentType { get; set; }

    [Display("Status"), StaticDataSource(typeof(ContentStatusDataSource))]
    public string? Status { get; set; }

    [Display("Space ID"), DataSource(typeof(SpaceDataSource))]
    public string? SpaceId { get; set; } = string.Empty;

    [Display("Created from")]
    public DateTime? CreatedFrom { get; set; }
    
    [Display("Updated from")]
    public DateTime? UpdatedFrom { get; set; }

    [Display("Parent ID")]
    public string? ParentId { get; set; }

    [Display("CQL query")]
    public string? CqlQuery { get; set; }
}