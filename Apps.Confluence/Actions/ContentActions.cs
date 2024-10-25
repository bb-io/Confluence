using System.Text;
using Apps.Confluence.Api;
using Apps.Confluence.Invocables;
using Apps.Confluence.Models.Identifiers;
using Apps.Confluence.Models.Requests.Content;
using Apps.Confluence.Models.Responses.Content;
using Apps.Confluence.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using RestSharp;

namespace Apps.Confluence.Actions;

[ActionList]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : AppInvocable(invocationContext)
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
        var apiRequest = new ApiRequest($"/api/content/{request.ContentId}?expand=body.view,version,space", Method.Get, Creds);
        return await Client.ExecuteWithErrorHandling<ContentResponse>(apiRequest);
    }
    
    [Action("Get content as HTML", Description = "Returns a single content HTML specified by the content ID.")]
    public async Task<GetContentAsHtmlResponse> GetContentAsHtmlAsync([ActionParameter] ContentIdentifier request)
    {
        var apiRequest = new ApiRequest($"/api/content/{request.ContentId}?expand=body.view", Method.Get, Creds);
        var response = await Client.ExecuteWithErrorHandling<ContentResponse>(apiRequest);
        
        var html = HtmlConverter.ConvertToHtml(response);
        var fileMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(html));
        fileMemoryStream.Position = 0;
        
        var fileReference = await fileManagementClient.UploadAsync(fileMemoryStream, "text/html", $"content-{request.ContentId}.html");
        return new()
        {
            File = fileReference
        };
    }
    
    [Action("Update content from HTML", Description = "Updates a content from HTML file.")]
    public async Task UpdateContentFromHtmlAsync([ActionParameter] UpdateContentFromHtmlRequest request)
    {
        var stream = await fileManagementClient.DownloadAsync(request.File);
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        var htmlString = Encoding.UTF8.GetString(memoryStream.ToArray());
        var htmlEntity = HtmlConverter.ExtractHtmlContent(htmlString);
        var contentId = request.ContentId ?? htmlEntity.Id 
            ?? throw new Exception("Could not find content ID. Please provide it in the inputs.");
        
        var content = await GetContentAsync(new ContentIdentifier()
        {
            ContentId = contentId
        });
        
        var updateRequest = new ApiRequest($"/api/content/{contentId}", Method.Put, Creds)
            .WithJsonBody(new
            {
                type = content.Type,
                title = htmlEntity.Title,
                body = new
                {
                    storage = new
                    {
                        value = htmlEntity.HtmlContent,
                        representation = "storage"
                    }
                },
                version = new
                {
                    number = content.Version.Number + 1
                }
            });
        
        await Client.ExecuteWithErrorHandling(updateRequest);
    }
    
    [Action("Create content", Description = "Creates a new piece of content.")]
    public async Task<ContentResponse> CreateContentAsync([ActionParameter] CreateContentRequest request)
    {
        var bodyDictionary = new Dictionary<string, object>
        {
            { "type", request.Type }
        };
        
        if (!string.IsNullOrEmpty(request.Title))
        {
            bodyDictionary.Add("title", request.Title);
        }
        
        if (!string.IsNullOrEmpty(request.SpaceId))
        {
            bodyDictionary.Add("space", new
            {
                id = Convert.ToInt32(request.SpaceId)
            });
        }
        
        if (!string.IsNullOrEmpty(request.Body))
        {
            bodyDictionary.Add("body", new
            {
                storage = new
                {
                    value = request.Body,
                    representation = "storage"
                }
            });
        }
        
        if(!string.IsNullOrEmpty(request.Status))
        {
            bodyDictionary.Add("status", new
            {
                value = request.Status
            });
        }
        
        var apiRequest = new ApiRequest("/api/content", Method.Post, Creds)
            .WithJsonBody(bodyDictionary);
        
        return await Client.ExecuteWithErrorHandling<ContentResponse>(apiRequest);
    }
    
    [Action("Delete content", Description = "Deletes a piece of content.")]
    public async Task DeleteContentAsync([ActionParameter] ContentIdentifier request)
    {
        var apiRequest = new ApiRequest($"/api/content/{request.ContentId}", Method.Delete, Creds);
        await Client.ExecuteWithErrorHandling(apiRequest);
    }
}