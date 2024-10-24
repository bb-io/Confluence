using Apps.Confluence.Api;
using Apps.Confluence.Invocables;
using Apps.Confluence.Models.Identifiers;
using Apps.Confluence.Models.Requests.Content;
using Apps.Confluence.Models.Responses.Content;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Confluence.Actions;

[ActionList]
public class ContentActions(InvocationContext invocationContext) : AppInvocable(invocationContext)
{
    [Action("Search content", Description = "Returns a list of content that matches the search criteria.")]
    public async Task<SearchContentResponse> SearchContentAsync([ActionParameter] FilterContentRequest request)
    {
        var apiRequest = new ApiRequest("/api/content", Method.Get, Creds);
        return await Client.ExecuteWithErrorHandling<SearchContentResponse>(apiRequest);
    }
    
    [Action("Get content", Description = "Returns a single content object specified by the content ID.")]
    public async Task<ContentResponse> GetContentAsync([ActionParameter] ContentIdentifier request)
    {
        var apiRequest = new ApiRequest($"/api/content/{request.ContentId}", Method.Get, Creds);
        return await Client.ExecuteWithErrorHandling<ContentResponse>(apiRequest);
    }
}