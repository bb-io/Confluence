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
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;
using System.Text;

namespace Apps.Confluence.Actions;

[ActionList("Content")]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : AppInvocable(invocationContext)
{
    [BlueprintActionDefinition(BlueprintAction.SearchContent)]
    [Action("Search content", Description = "Returns a list of content that matches the search criteria.")]
    public async Task<SearchContentResponse> SearchContentAsync([ActionParameter] FilterContentRequest request)
    {
        var allResults = new List<ContentResponse>();
        var start = 0;
        var limit = 100;
        var maxIterations = 1000;
        var currentIteration = 0;

        var validContentTypes = new HashSet<string> { "page", "blogpost", "comment", "attachment" };
        var cqlParts = new List<string>();

        if (!string.IsNullOrEmpty(request.ContentType))
        {
            if (!validContentTypes.Contains(request.ContentType.ToLower()))
            {
                throw new PluginMisconfigurationException($"Invalid content type: {request.ContentType}. Valid types are: {string.Join(", ", validContentTypes)}");
            }
            cqlParts.Add($"type={request.ContentType.ToLower()}");
        }

        if (request.CreatedFrom.HasValue)
        {
            var createdDate = request.CreatedFrom.Value.ToUniversalTime();
            if (createdDate > DateTime.UtcNow)
            {
                throw new PluginMisconfigurationException($"CreatedFrom date ({createdDate}) cannot be in the future.");
            }
            cqlParts.Add($"created>=\"{createdDate:yyyy-MM-dd}\"");
        }

        //if (!string.IsNullOrEmpty(request.SpaceId))
        //{
        //    cqlParts.Add($"space = {request.SpaceId}");
        //}

        if (!string.IsNullOrEmpty(request.ParentId))
        {
            cqlParts.Add($"parent = {request.ParentId}");
        }

        if (request.UpdatedFrom.HasValue)
        {
            var updatedDate = request.UpdatedFrom.Value.ToUniversalTime();
            if (updatedDate > DateTime.UtcNow)
            {
                throw new PluginMisconfigurationException($"UpdatedFrom date ({updatedDate}) cannot be in the future.");
            }
            cqlParts.Add($"lastModified>=\"{updatedDate:yyyy-MM-dd}\"");
        }

        if (!string.IsNullOrEmpty(request.CqlQuery))
        {
            var trimmedCqlQuery = request.CqlQuery.Trim();
            if (string.IsNullOrWhiteSpace(trimmedCqlQuery))
            {
                throw new PluginMisconfigurationException("CQL query cannot be empty or whitespace.");
            }
            if (trimmedCqlQuery.Contains(";") || trimmedCqlQuery.Contains("--"))
            {
                throw new PluginMisconfigurationException("CQL query contains invalid characters (e.g., ';' or '--').");
            }
            cqlParts.Add($"({trimmedCqlQuery})");
        }

        var cql = cqlParts.Any() ? string.Join(" AND ", cqlParts) : "type IN (page,blogpost,comment)";

        while (currentIteration < maxIterations)
        {
            currentIteration++;

            var endpoint = "/rest/api/content/search";
            var apiRequest = new ApiRequest(endpoint, Method.Get, Creds)
                .AddParameter("cql", cql, ParameterType.QueryString)
                .AddParameter("start", start, ParameterType.QueryString)
                .AddParameter("limit", limit, ParameterType.QueryString)
                .AddParameter("expand", "ancestors,body.view,version,space,history,history.lastUpdated", ParameterType.QueryString);
            
            try
            {
                var response = await Client.ExecuteWithErrorHandling<SearchContentResponse>(apiRequest);

                if (response == null)
                {
                    break;
                }

                if (response.Results != null && response.Results.Any())
                {
                    var filteredResults = string.IsNullOrEmpty(request.Status)
                        ? response.Results
                        : response.Results.Where(r => r.Status == request.Status).ToList();

                    if (filteredResults.Any())
                    {
                        allResults.AddRange(filteredResults);
                    }

                    if (response.Results.Count < limit)
                    {
                        break;
                    }

                    var hasNextPage = false;
                    if (response.Links != null)
                    {
                        hasNextPage = !string.IsNullOrEmpty(response.Links.Next);
                    }

                    if (!hasNextPage)
                    {
                        break;
                    }

                    var newStart = start + limit;
                    if (newStart <= start)
                    {
                        break;
                    }
                    start = newStart;
                }
                else
                {

                    break;
                }
                if (response.Size.HasValue && start >= response.Size.Value)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                break;
            }
        }

        if (!string.IsNullOrEmpty(request.SpaceId))
        {
            allResults = allResults
                .Where(r => r.Space?.Id == request.SpaceId)
                .ToList();
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
            .AddParameter("expand", "ancestors,body.view,version,space,history,history.lastUpdated", ParameterType.QueryString)
            .AddParameter("limit", 1, ParameterType.QueryString);

        var response = await Client.ExecuteWithErrorHandling<SearchContentResponse>(apiRequest);

        if (response.Results == null || !response.Results.Any())
        {
            throw new PluginApplicationException($"Content with ID {request.ContentId} not found.");
        }

        return response.Results.First();
    }

    [BlueprintActionDefinition(BlueprintAction.DownloadContent)]
    [Action("Download content", Description = "Returns a single content HTML specified by the content ID.")]
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

    [BlueprintActionDefinition(BlueprintAction.UploadContent)]
    [Action("Create content from HTML", Description = "Create content from an HTML file.")]
    public async Task<ContentResponse> CreateContentFromHtmlAsync(
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

        var htmlString = Encoding.UTF8.GetString(memoryStream.ToArray());
        var htmlEntity = HtmlConverter.ExtractHtmlContent(htmlString);

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

        var contentResponse = await Client.ExecuteWithErrorHandling<ContentResponse>(apiRequest);
        return await GetContentAsync(new ContentIdentifier { ContentId = contentResponse.Id });
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