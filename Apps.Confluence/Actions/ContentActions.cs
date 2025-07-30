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
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Confluence.Actions;

[ActionList]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : AppInvocable(invocationContext)
{
    [Action("Search content", Description = "Returns a list of content that matches the search criteria.")]
    public async Task<SearchContentResponse> SearchContentAsync([ActionParameter] FilterContentRequest request)
    {
        var allResults = new List<ContentResponse>();
        var start = 0;
        var limit = 25;

        var cqlParts = new List<string>();
        if (!string.IsNullOrEmpty(request.ContentType))
        {
            cqlParts.Add($"type={request.ContentType}");
        }
        if (request.CreatedFrom.HasValue)
        {
            cqlParts.Add($"created>=\"{request.CreatedFrom.Value.ToUniversalTime():yyyy-MM-dd'T'HH:mm:ss'Z'}\"");
        }
        if (request.UpdatedFrom.HasValue)
        {
            cqlParts.Add($"lastModified>=\"{request.UpdatedFrom.Value.ToUniversalTime():yyyy-MM-dd'T'HH:mm:ss'Z'}\"");
        }

        var cql = cqlParts.Any() ? string.Join(" AND ", cqlParts) : "type IN (page,blogpost,comment)"; 

        while (true)
        {
            var endpoint = "/rest/api/content/search";

            var apiRequest = new ApiRequest(endpoint, Method.Get, Creds)
                .AddParameter("cql", cql, ParameterType.QueryString)
                .AddParameter("start", start, ParameterType.QueryString)
                .AddParameter("limit", limit, ParameterType.QueryString)
                .AddParameter("expand", "body.view,version,space,history,history.lastUpdated", ParameterType.QueryString);

            var response = await Client.ExecuteWithErrorHandling<SearchContentResponse>(apiRequest);

            if (response.Results != null && response.Results.Any())
            {
                var filteredResults = string.IsNullOrEmpty(request.Status)
                    ? response.Results
                    : response.Results.Where(r => r.Status == request.Status).ToList();

                allResults.AddRange(filteredResults);
            }

            if (response.Size < limit)
            {
                break;
            }

            start += limit;
        }

        return new SearchContentResponse
        {
            Results = allResults,
            Start = 0,
            Limit = allResults.Count,
            Size = allResults.Count
        };
    }

    [Action("Get content", Description = "Returns a single content object specified by the content ID.")]
    public async Task<ContentResponse> GetContentAsync([ActionParameter] ContentIdentifier request)
    {
        var endpoint = "/rest/api/content/search";
        var cql = $"id={request.ContentId}";
        var apiRequest = new ApiRequest(endpoint, Method.Get, Creds)
            .AddParameter("cql", cql, ParameterType.QueryString)
            .AddParameter("expand", "body.view,version,space,history,history.lastUpdated", ParameterType.QueryString)
            .AddParameter("limit", 1, ParameterType.QueryString);

        var response = await Client.ExecuteWithErrorHandling<SearchContentResponse>(apiRequest);

        if (response.Results == null || !response.Results.Any())
        {
            throw new PluginApplicationException($"Content with ID {request.ContentId} not found.");
        }

        return response.Results.First();
    }

    [Action("Get content as HTML", Description = "Returns a single content HTML specified by the content ID.")]
    public async Task<GetContentAsHtmlResponse> GetContentAsHtmlAsync([ActionParameter] ContentIdentifier request)
    {
        var endpoint = "/rest/api/content/search";
        var cql = $"id={request.ContentId}";
        var apiRequest = new ApiRequest(endpoint, Method.Get, Creds)
            .AddParameter("cql", cql, ParameterType.QueryString)
            .AddParameter("expand", "body.view", ParameterType.QueryString)
            .AddParameter("limit", 1, ParameterType.QueryString);

        var response = await Client.ExecuteWithErrorHandling<SearchContentResponse>(apiRequest);

        if (response.Results == null || !response.Results.Any())
        {
            throw new PluginApplicationException($"Content with ID {request.ContentId} not found.");
        }

        var content = response.Results.First();
        var html = HtmlConverter.ConvertToHtml(content);
        var fileMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(html));
        fileMemoryStream.Position = 0;

        var fileReference = await fileManagementClient.UploadAsync(fileMemoryStream, "text/html", $"content-{request.ContentId}.html");
        return new()
        {
            File = fileReference
        };
    }

    [Action("Create content from HTML", Description = "Create a content from HTML file.")]
    public async Task<ContentResponse> UpdateContentFromHtmlAsync(
        [ActionParameter] UpdateContentFromHtmlRequest request)
    {
        var stream = await fileManagementClient.DownloadAsync(request.File);
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var htmlString = Encoding.UTF8.GetString(memoryStream.ToArray());
        var htmlEntity = HtmlConverter.ExtractHtmlContent(htmlString);

        var updateRequest = new ApiRequest($"/api/content", Method.Post, Creds)
            .WithJsonBody(new
            {
                type = request.ContentType ?? "page",
                title = htmlEntity.Title,
                status = "current",
                body = new
                {
                    storage = new
                    {
                        value = htmlEntity.HtmlContent,
                        representation = "storage"
                    }
                },
                space = new
                {
                    id = Convert.ToInt32(request.SpaceId)
                }
            });

        var contentResponse = await Client.ExecuteWithErrorHandling<ContentResponse>(updateRequest);
        return await GetContentAsync(new ContentIdentifier { ContentId = contentResponse.Id });
    }

    [Action("Update content from HTML", Description = "Updates a content from HTML file.")]
    public async Task UpdateContentFromHtmlWithNotExtstingContentAsync(
        [ActionParameter] UpdateContentRequest request)
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
                }
            );
        
        await Client.ExecuteWithErrorHandling(updateRequest);
    }

    [Action("Create content", Description = "Creates a new content with specified data.")]
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

        bodyDictionary.Add("body", new
        {
            storage = new
            {
                value = request.Body ?? string.Empty,
                representation = "storage"
            }
        });

        if (!string.IsNullOrEmpty(request.Status))
        {
            bodyDictionary.Add("status", request.Status);
        }

        var apiRequest = new ApiRequest("/api/content", Method.Post, Creds)
            .WithJsonBody(bodyDictionary);

        return await Client.ExecuteWithErrorHandling<ContentResponse>(apiRequest);
    }

    [Action("Delete content", Description = "Deletes a piece of content.")]
    public async Task DeleteContentAsync([ActionParameter] ContentIdentifier request)
    {
        var content = await GetContentAsync(new ContentIdentifier { ContentId=request.ContentId});
        var endpoint=string.Empty;

        if (content.Type == "page")
        {
            endpoint = $"api/v2/pages/{request.ContentId}";
        }
        else if (content.Type == "blogpost")
        {
            endpoint = $"api/v2/blogposts/{request.ContentId}";
        }
        else if (content.Type == "comment")
        {
            endpoint = $"api/v2/inline-comments/{request.ContentId}";
        }
        else if (content.Type == "attachment")
        {
            endpoint = $"api/v2/attachments/{request.ContentId}";
        }
        else if (content.Type == "attachment")
        {
            endpoint = $"api/v2/attachments/{request.ContentId}";
        }
        else if (content.Type == "whiteboard")
        {
            endpoint = $"api/v2/whiteboards/{request.ContentId}";
        }
        else if (content.Type == "embed")
        {
            endpoint = $"api/v2/embeds/{request.ContentId}";
        }

        var apiRequest = new ApiRequest(endpoint, Method.Delete, Creds);
        await Client.ExecuteWithErrorHandling(apiRequest);
    }
}