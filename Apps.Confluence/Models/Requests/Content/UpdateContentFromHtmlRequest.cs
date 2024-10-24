using Apps.Confluence.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Confluence.Models.Requests.Content;

public class UpdateContentFromHtmlRequest : ContentOptionalIdentifier
{
    public FileReference File { get; set; } = default!;
}