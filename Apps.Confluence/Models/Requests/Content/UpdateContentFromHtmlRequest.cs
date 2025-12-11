using Apps.Confluence.DataSourceHandlers;
using Apps.Confluence.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Confluence.Models.Requests.Content;

public class CreateContentFromHtmlRequest
{
    public FileReference File { get; set; } = default!;

    [Display("Space ID"), DataSource(typeof(SpaceDataSource))]
    public string SpaceId { get; set; } = default!;

    [Display("Content type", Description = "By default this value will be set to 'page'"), StaticDataSource(typeof(ContentTypeDataSource))]
    public string? ContentType { get; set; }

    [Display("Parent ID", Description = "Only for pages"), DataSource(typeof(ContentDataSource))]
    public string? ParentId { get; set; }
}