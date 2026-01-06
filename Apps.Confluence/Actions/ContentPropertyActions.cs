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
using Apps.Confluence.Models.Responses.Properties;
using Apps.Confluence.Models.Requests.Properties;

namespace Apps.Confluence.Actions;

[ActionList("Properties")]
public class ContentPropertyActions(InvocationContext invocationContext )
    : AppInvocable(invocationContext)
{

    //[Action("Search content properties", Description = "Lists all content properties for a content ID")]
    //public async Task<ContentPropertyListResponse> SearchContentPropertiesAsync([ActionParameter] ContentIdentifier request)
    //{
    //    var endpoint = $"/rest/api/content/{request.ContentId}/property";

    //    var apiRequest = new ApiRequest(endpoint, Method.Get, Creds);

    //    return await Client.ExecuteWithErrorHandling<ContentPropertyListResponse>(apiRequest);
    //}

    //[Action("Get content property", Description = "Gets a specific content property by key")]
    //public async Task<ContentPropertyResponse> GetContentPropertyAsync([ActionParameter] PropertyIdentifier request)
    //{
    //    var endpoint = $"/rest/api/content/{request.ContentId}/property/{request.Key}";

    //    var apiRequest = new ApiRequest(endpoint, Method.Get, Creds);

    //    return await Client.ExecuteWithErrorHandling<ContentPropertyResponse>(apiRequest);
    //}

    [Action("Set page property", Description = "Creates or updates a page property")]
    public async Task<ContentPropertyResponse> SetPagePropertyAsync([ActionParameter] SetPropertyRequest request)
    {
        var searchEndpoint =
            $"/wiki/api/v2/pages/{request.ContentId}/properties?key={Uri.EscapeDataString(request.Key)}";

        var searchRequest = new ApiRequest(searchEndpoint, Method.Get, Creds);

        var searchResult =
            await Client.ExecuteWithErrorHandling<ContentPropertyListResponse>(searchRequest);

        var existing = searchResult.Results?.FirstOrDefault();

        if (existing is null)
        {
            var createEndpoint = $"/wiki/api/v2/pages/{request.ContentId}/properties";

            var createBody = new
            {
                key = request.Key,
                value = request.Value,
                version = new { number = 1 }
            };

            var apiRequest = new ApiRequest(createEndpoint, Method.Post, Creds)
                .WithJsonBody(createBody);

            return await Client.ExecuteWithErrorHandling<ContentPropertyResponse>(apiRequest);
        }
        else
        {
            var updateEndpoint =
                $"/wiki/api/v2/pages/{request.ContentId}/properties/{existing.Id}";

            var updateBody = new
            {
                key = request.Key,
                value = request.Value,
                version = new { number = existing.Version.Number + 1 }
            };

            var apiRequest = new ApiRequest(updateEndpoint, Method.Put, Creds)
                .WithJsonBody(updateBody);

            return await Client.ExecuteWithErrorHandling<ContentPropertyResponse>(apiRequest);
        }
    }

    [Action("Get page property", Description = "Gets a specific page property")]
    public async Task<ContentPropertyResponse> GetPagePropertyAsync([ActionParameter] PropertyIdentifier request)
    {
        var endpoint =
            $"/wiki/api/v2/pages/{request.ContentId}/properties?key={Uri.EscapeDataString(request.Key)}";

        var apiRequest = new ApiRequest(endpoint, Method.Get, Creds);

        var result =
            await Client.ExecuteWithErrorHandling<ContentPropertyListResponse>(apiRequest);

        var property = result.Results?.FirstOrDefault();

        if (property == null)
            throw new PluginApplicationException(
                $"Property '{request.Key}' not found for page {request.ContentId}");

        return property;
    }



    //[Action("Set content property", Description = "Creates or updates a content property")]
    //public async Task<ContentPropertyResponse> SetContentPropertyAsync([ActionParameter] SetPropertyRequest request)
    //{
    //    var endpoint = $"/rest/api/content/{request.ContentId}/property/{request.Key}";

    //    int versionNumber = 1;

    //    // Try fetch existing to get current version
    //    try
    //    {
    //        var existing = await GetContentPropertyAsync(new PropertyIdentifier
    //        {
    //            ContentId = request.ContentId,
    //            Key = request.Key
    //        });

    //        versionNumber = existing.Version.Number + 1;
    //    }
    //    catch
    //    {
    //        // Property does not exist → stay at version 1
    //    }

    //    var body = new
    //    {
    //        value = request.Value,
    //        version = new { number = versionNumber }
    //    };

    //    var apiRequest = new ApiRequest(endpoint, Method.Put, Creds)
    //        .WithJsonBody(body);

    //    return await Client.ExecuteWithErrorHandling<ContentPropertyResponse>(apiRequest);
    //}


}