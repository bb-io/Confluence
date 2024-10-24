using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Confluence.Models.Responses.Content;

public class GetContentAsHtmlResponse
{
    public FileReference File { get; set; } = default!;
}