using Apps.Confluence.Models.Responses.Content;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Confluence.Polling.Models.Response;

public record OnContentCreatedResponse(SearchContentResponse Content) : IMultiDownloadableContentOutput<ContentResponse>
{
    public List<ContentResponse> Items { get; set; } = Content.Results;
}
