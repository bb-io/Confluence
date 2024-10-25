using Apps.Confluence.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Confluence.Models.Requests.Content;

public class FilterContentRequest
{
    [Display("Content type"), StaticDataSource(typeof(ContentTypeDataSource))]
    public string? ContentType { get; set; }

    [Display("Status"), StaticDataSource(typeof(ContentStatusDataSource))]
    public string? Status { get; set; }
}