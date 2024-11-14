using Apps.Confluence.DataSourceHandlers;
using Apps.Confluence.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Confluence.Polling.Models.Requests;

public class FilterContentPollingRequest
{
    [Display("Content type"), StaticDataSource(typeof(ContentTypeDataSource))]
    public string? ContentType { get; set; }

    [Display("Status"), StaticDataSource(typeof(ContentStatusDataSource))]
    public string? Status { get; set; }

    [Display("Space ID"), StaticDataSource(typeof(SpaceDataSource))]
    public string? SpaceId { get; set; }
}