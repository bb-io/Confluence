using Apps.Confluence.Api;
using Apps.Confluence.Invocables;
using Apps.Confluence.Models.Identifiers;
using Apps.Confluence.Models.Requests.Content;
using Apps.Confluence.Models.Responses.Content;
using Apps.Confluence.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Filters.Transformations;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;
using System.Text;
using Apps.Confluence.Constants;
using Apps.Confluence.Extensions;
using Apps.Confluence.Models.Entities.Content;
using Blackbird.Filters.Xliff.Xliff2;
using Blackbird.Filters.Xliff.Xliff1;

namespace Apps.Confluence.Actions;

[ActionList("Content")]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : AppInvocable(invocationContext)
{
    [BlueprintActionDefinition(BlueprintAction.SearchContent)]
    [Action("Search content", Description = "Returns a list of content that matches the search criteria.")]
    public async Task<SearchContentResponse> SearchContentAsync([ActionParameter] FilterContentRequest input)
    {
        input.Validate();

        var cqlParts = new List<string>();

        if (!string.IsNullOrEmpty(input.ContentType))
            cqlParts.Add($"type={input.ContentType.ToLower()}");

        if (input.CreatedFrom.HasValue)
            cqlParts.Add($"created>=\"{input.CreatedFrom.Value.ToUniversalTime().AddDays(-1):yyyy-MM-dd}\"");

        if (!string.IsNullOrEmpty(input.ParentId))
            cqlParts.Add($"parent = {input.ParentId}");

        if (!string.IsNullOrEmpty(input.SpaceId))
        {
            string spaceKey = await Client.GetSpaceKey(Creds, input.SpaceId);
            cqlParts.Add($"space = \"{spaceKey}\"");
        }

        if (input.UpdatedFrom.HasValue)
            cqlParts.Add($"lastModified>=\"{input.UpdatedFrom.Value.ToUniversalTime().AddDays(-1):yyyy-MM-dd}\"");

        if (!string.IsNullOrEmpty(input.CqlQuery))
        {
            var trimmed = input.CqlQuery.Trim();
            cqlParts.Add($"({trimmed})");
        }

        string cql = cqlParts.Count != 0 ? string.Join(" AND ", cqlParts) : CqlQueries.EditableContentCql;
        var request = new ApiRequest("/rest/api/content/search", Method.Get, Creds)
            .AddParameter("cql", cql, ParameterType.QueryString)
            .AddParameter("expand", "ancestors,space,history,history.lastUpdated", ParameterType.QueryString);

        var contentEntities = await Client.Paginate<ContentEntity>(request);
        contentEntities = contentEntities
            .Where(x => string.IsNullOrEmpty(input.Status) || x.Status == input.Status)
            .Where(x => !input.CreatedFrom.HasValue || x.History?.CreatedDate.ToUniversalTime() >= input.CreatedFrom.Value.ToUniversalTime())
            .Where(x => !input.UpdatedFrom.HasValue || x.History?.LastUpdated.When.ToUniversalTime() >= input.UpdatedFrom.Value.ToUniversalTime())
            .ToList();

        return new(contentEntities.Select(x => new ContentResponse(x)).ToList());
    }
    
    [Action("Get content", Description = "Returns a single content object specified by the content ID.")]
    public async Task<ContentEntity> GetContentAsync([ActionParameter] ContentIdentifier request)
    {
        var endpoint = "/rest/api/content/search";
        var cql = $"id={request.ContentId}";
        var apiRequest = new ApiRequest(endpoint, Method.Get, Creds)
            .AddParameter("cql", cql, ParameterType.QueryString)
            .AddParameter("expand", "ancestors,body.view,version,space,history,history.lastUpdated", ParameterType.QueryString);

        var response = await Client.Paginate<ContentEntity>(apiRequest, maxPages: 1);
        if (response.Count == 0)
            throw new PluginMisconfigurationException($"Content with ID {request.ContentId} not found.");

        return response.First();
    }

    [BlueprintActionDefinition(BlueprintAction.DownloadContent)]
    [Action("Download content", Description = "Returns a single content HTML specified by the content ID.")]
    public async Task<GetContentAsHtmlResponse> GetContentAsHtmlAsync([ActionParameter] ContentIdentifier request)
    {
        var endpoint = "/rest/api/content/search";
        var cql = $"id={request.ContentId}";
        var apiRequest = new ApiRequest(endpoint, Method.Get, Creds)
            .AddParameter("cql", cql, ParameterType.QueryString)
            .AddParameter("expand", "body.storage,body.view", ParameterType.QueryString);
        
        var response = await Client.Paginate<ContentEntity>(apiRequest, maxPages: 1);
        if (response.Count == 0)
            throw new PluginMisconfigurationException($"Content with ID {request.ContentId} not found.");

        var content = response.First();
        var html = HtmlConverter.ConvertToHtml(content);
        var fileMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(html));
        fileMemoryStream.Position = 0;

        var fileReference = await fileManagementClient.UploadAsync(fileMemoryStream, "text/html", $"content-{request.ContentId}.html");
        return new()
        {
            File = fileReference
        };
    }

    [BlueprintActionDefinition(BlueprintAction.UploadContent)]
    [Action("Create content from HTML", Description = "Create content from an HTML file.")]
    public async Task<NewContentResponse> CreateContentFromHtmlAsync(
        [ActionParameter] CreateContentFromHtmlRequest request)
    {
        if (string.IsNullOrEmpty(request.ContentType))
        {
            throw new PluginApplicationException("Content type is required.");
        }

        if (string.IsNullOrEmpty(request.SpaceId))
        {
            throw new PluginApplicationException("Space ID is required for creating content.");
        }

        var stream = await fileManagementClient.DownloadAsync(request.File);
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var fileString = Encoding.UTF8.GetString(memoryStream.ToArray());

        Transformation? transformation = null;
        if (Xliff2Serializer.IsXliff2(fileString) || Xliff1Serializer.IsXliff1(fileString))
        {
            transformation = Transformation.Parse(fileString, request.File.Name);
            fileString = transformation.Target().Serialize() ?? throw new PluginMisconfigurationException("XLIFF did not contain files");
        }
      
        var htmlEntity = HtmlConverter.ExtractHtmlContent(fileString);

        var bodyDictionary = new Dictionary<string, object>
        {
            { "spaceId", request.SpaceId },
            { "title", htmlEntity.Title ?? string.Empty },
            { "status", "current" },
            { "body", new
                {
                    storage = new
                    {
                        value = htmlEntity.HtmlContent ?? string.Empty,
                        representation = "storage"
                    }
                }
            }
        };

        if (request.ContentType.Equals("page", StringComparison.OrdinalIgnoreCase)
       && !string.IsNullOrEmpty(request.ParentId))
        {
            bodyDictionary["parentId"] = request.ParentId;
        }

        var endpoint = string.Empty;
        switch (request.ContentType.ToLower())
        {
            case "page":
                endpoint = "/api/v2/pages";
                break;
            case "blogpost":
                endpoint = "/api/v2/blogposts";
                break;
            case "comment":
                endpoint = "/api/v2/inline-comments";
                break;
            default:
                throw new PluginMisconfigurationException($"Unsupported content type: {request.ContentType}");
        }

        var apiRequest = new ApiRequest(endpoint, Method.Post, Creds)
            .WithJsonBody(bodyDictionary);

        var contentResponse = await Client.ExecuteWithErrorHandling<ContentEntity>(apiRequest);
        return new NewContentResponse {Id = contentResponse.ContentId }; 
       // return await GetContentAsync(new ContentIdentifier { ContentId = contentResponse.Id });
    }

    [Action("Update content from HTML", Description = "Updates a content from HTML file.")]
    public async Task UpdateContentFromHtml(
        [ActionParameter] UpdateContentRequest request)
    {
        var stream = await fileManagementClient.DownloadAsync(request.File);
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var htmlString = Encoding.UTF8.GetString(memoryStream.ToArray());
        var htmlEntity = HtmlConverter.ExtractHtmlContent(htmlString);

        var contentId = request.ContentId;
        if (string.IsNullOrEmpty(contentId))
        {
            if (string.IsNullOrEmpty(htmlEntity.Id))
            {
                throw new PluginApplicationException("Could not find content ID. Please provide it in the inputs or HTML meta tag.");
            }
            contentId = htmlEntity.Id;
        }

        var content = await GetContentAsync(new ContentIdentifier { ContentId = contentId });

        var endpoint = string.Empty;
        switch (content.Type.ToLower())
        {
            case "page":
                endpoint = $"/api/v2/pages/{contentId}";
                break;
            case "blogpost":
                endpoint = $"/api/v2/blogposts/{contentId}";
                break;
            case "comment":
                endpoint = $"/api/v2/inline-comments/{contentId}";
                break;
            default:
                throw new PluginApplicationException($"Unsupported content type: {content.Type}");
        }

        var bodyDictionary = new Dictionary<string, object>
        {
            { "id", contentId },
            { "type", content.Type },
            { "title", htmlEntity.Title ?? content.Title },
            { "status", "current" },
            { "body", new
                {
                    value = htmlEntity.HtmlContent ?? content.Body.View.Value,
                    representation = "storage"
                }
            },
            { "version", new
                {
                    number = content.Version.Number + 1
                }
            }
        };

        var updateRequest = new ApiRequest(endpoint, Method.Put, Creds)
            .WithJsonBody(bodyDictionary);

        await Client.ExecuteWithErrorHandling(updateRequest);
    }

    [Action("Create content", Description = "Creates a new content with specified data.")]
    public async Task<CreateContentResponse> CreateContentAsync([ActionParameter] CreateContentRequest request)
    {
        if (string.IsNullOrEmpty(request.Type))
        {
            throw new PluginApplicationException("Content type is required.");
        }

        if (string.IsNullOrEmpty(request.SpaceId))
        {
            throw new PluginApplicationException("SpaceId is required for creating content.");
        }

        var bodyDictionary = new Dictionary<string, object>();

        var endpoint = string.Empty;
        switch (request.Type.ToLower())
        {
            case "page":
                endpoint = "/api/v2/pages";
                break;
            case "blogpost":
                endpoint = "/api/v2/blogposts";
                break;
            case "comment":
                endpoint = "/api/v2/inline-comments";
                break;
            default:
                throw new PluginApplicationException($"Unsupported content type: {request.Type}");
        }

        if (!string.IsNullOrEmpty(request.Title))
        {
            bodyDictionary.Add("title", request.Title);
        }

        bodyDictionary.Add("spaceId", request.SpaceId); 

        bodyDictionary.Add("body", new
        {
            value = request.Body ?? string.Empty,
            representation = "storage"
        });

        if (!string.IsNullOrEmpty(request.Status))
        {
            bodyDictionary.Add("status", request.Status);
        }

        var apiRequest = new ApiRequest(endpoint, Method.Post, Creds)
            .WithJsonBody(bodyDictionary);

        return await Client.ExecuteWithErrorHandling<CreateContentResponse>(apiRequest);
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
       
        var apiRequest = new ApiRequest(endpoint, Method.Delete, Creds);
        await Client.ExecuteWithErrorHandling(apiRequest);
    }
}