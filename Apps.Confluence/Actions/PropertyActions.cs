using Apps.Confluence.Api;
using Apps.Confluence.Invocables;
using Apps.Confluence.Models.Identifiers;
using Apps.Confluence.Models.Requests.Content;
using Apps.Confluence.Models.Requests.Properties;
using Apps.Confluence.Models.Responses.Content;
using Apps.Confluence.Models.Responses.Properties;
using Apps.Confluence.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Transformations;
using RestSharp;
using System.Collections.Generic;
using System.Text;

namespace Apps.Confluence.Actions;

[ActionList("Properties")]
public class ContentPropertyActions(InvocationContext invocationContext )
    : AppInvocable(invocationContext)
{

    [Action("Set page property", Description = "Creates or updates a page property")]
    public async Task<ContentPropertyResponse> SetPagePropertyAsync([ActionParameter] SetPropertyRequest request)
    {
        var getEndpoint = $"/api/v2/pages/{request.ContentId}/properties";
        var getRequest = new ApiRequest(getEndpoint, Method.Get, Creds);

        var getResult = await Client.ExecuteWithErrorHandling<ContentPropertyListResponse>(getRequest);

        var existing = getResult.Results?
            .FirstOrDefault(p =>
                string.Equals(p.Key, request.Key, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            var createEndpoint = $"/api/v2/pages/{request.ContentId}/properties";

            var createBody = new
            {
                key = request.Key,
                value = request.Value
            };

            var apiRequest = new ApiRequest(createEndpoint, Method.Post, Creds)
                .WithJsonBody(createBody);

            return await Client.ExecuteWithErrorHandling<ContentPropertyResponse>(apiRequest);
        }

        var updateEndpoint =
            $"/api/v2/pages/{request.ContentId}/properties/{existing.Id}";

        var updateBody = new
        {
            key = request.Key,
            value = request.Value,
            version = new
            {
                number = existing.Version.Number + 1
            }
        };

        var updateRequest = new ApiRequest(updateEndpoint, Method.Put, Creds)
            .WithJsonBody(updateBody);

        return await Client.ExecuteWithErrorHandling<ContentPropertyResponse>(updateRequest);
    }


    [Action("Get page property", Description = "Gets a specific page property")]
    public async Task<ContentPropertyResponse> GetPagePropertyAsync([ActionParameter] PropertyIdentifier request)
    {
        var endpoint =
            $"/api/v2/pages/{request.ContentId}/properties?key={Uri.EscapeDataString(request.Key)}";

        var apiRequest = new ApiRequest(endpoint, Method.Get, Creds);

        var result =
            await Client.ExecuteWithErrorHandling<ContentPropertyListResponse>(apiRequest);

        var property = result.Results?.FirstOrDefault();

        if (property == null)
            throw new PluginApplicationException(
                $"Property '{request.Key}' not found for page {request.ContentId}");

        return property;
    }

    [Action("Search page properties", Description = "Returns page properties")]
    public async Task<ContentPropertyListResponse> SearchPagePropertiesAsync(
    [ActionParameter] ContentIdentifier contentId,
    [ActionParameter][Display("Filter by key (exact match)")] string? key = null)
    {
        var allResults = new List<ContentPropertyResponse>();

        var limit = 100;
        var start = 0;

        while (true)
        {
            var endpoint = $"/api/v2/pages/{contentId.ContentId}/properties?limit={limit}&start={start}";
            var request = new ApiRequest(endpoint, Method.Get, Creds);

            var result = await Client.ExecuteWithErrorHandling<ContentPropertyListResponse>(request);

            if (result.Results != null)
                allResults.AddRange(result.Results);

            if (result.Links?.Next == null || result.Results == null || result.Results.Count == 0)
                break;

            start += limit;
        }

        if (!string.IsNullOrWhiteSpace(key))
        {
            allResults = allResults
                .Where(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return new ContentPropertyListResponse
        {
            Results = allResults,
            Size = allResults.Count
        };
    }

}