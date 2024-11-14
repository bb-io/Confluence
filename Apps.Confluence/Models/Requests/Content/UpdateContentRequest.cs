using Apps.Confluence.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Confluence.Models.Requests.Content;

public class UpdateContentRequest
{
    public FileReference File { get; set; } = default!;

    [Display("Content ID"), DataSource(typeof(ContentDataSource))]
    public string? ContentId { get; set; }
}