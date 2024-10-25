using Apps.Confluence.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Confluence.Models.Identifiers;

public class ContentOptionalIdentifier
{
    [Display("Content ID", Description = "The identifier of the content object."), DataSource(typeof(ContentDataSource))]
    public string? ContentId { get; set; }
}