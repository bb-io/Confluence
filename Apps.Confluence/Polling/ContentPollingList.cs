using Apps.Confluence.Api;
using Apps.Confluence.Invocables;
using Apps.Confluence.Models.Requests.Content;
using Apps.Confluence.Models.Responses.Content;
using Apps.Confluence.Polling.Models;
using Apps.Confluence.Polling.Models.Requests;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using RestSharp;

namespace Apps.Confluence.Polling;

[PollingEventList]
public class ContentPollingList(InvocationContext invocationContext) : AppInvocable(invocationContext)
{
    [PollingEvent("On content created",
        "Polling event. Triggered after specified time interval and returns new content.")]
    public async Task<PollingEventResponse<DateMemory, SearchContentResponse>> OnContentCreated(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] FilterContentPollingRequest filterContentRequest)
    {
        if (request.Memory is null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new()
                {
                    LastInteractionDate = DateTime.UtcNow
                }
            };
        }

        var content = await GetContentWithoutPagination(new FilterContentRequest
        {
            CreatedFrom = request.Memory.LastInteractionDate, ContentType = filterContentRequest.ContentType,
            Status = filterContentRequest.Status
        });

        return new()
        {
            FlyBird = content.Results.Any(),
            Result = content,
            Memory = new()
            {
                LastInteractionDate = DateTime.UtcNow
            }
        };
    }

    [PollingEvent("On content updated",
        "Polling event. Triggered after specified time interval and returns updated content.")]
    public async Task<PollingEventResponse<DateMemory, SearchContentResponse>> OnContentUpdated(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] FilterContentPollingRequest filterContentRequest)
    {
        if (request.Memory is null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new()
                {
                    LastInteractionDate = DateTime.UtcNow
                }
            };
        }

        var content = await GetContentWithPagination(new FilterContentRequest
        {
            UpdatedFrom = request.Memory.LastInteractionDate, ContentType = filterContentRequest.ContentType,
            Status = filterContentRequest.Status
        });

        return new()
        {
            FlyBird = content.Results.Any(),
            Result = content,
            Memory = new()
            {
                LastInteractionDate = DateTime.UtcNow
            }
        };
    }

    private async Task<SearchContentResponse> GetContentWithoutPagination(FilterContentRequest request)
    {
        var endpoint = "/api/content?orderby=history.createdDate desc&expand=body.view,version,space";
        if (request.CreatedFrom.HasValue || request.UpdatedFrom.HasValue)
            endpoint += ",history,history.lastUpdated";

        var apiRequest = new ApiRequest(endpoint, Method.Get, Creds);
        AddRequestParameters(apiRequest, request);

        var response = await Client.ExecuteWithErrorHandling<SearchContentResponse>(apiRequest);
        FilterResults(response, request);

        return response;
    }

    private async Task<SearchContentResponse> GetContentWithPagination(FilterContentRequest request)
    {
        var allResults = new List<ContentResponse>();
        var start = 0;
        var limit = 25;

        while (true)
        {
            var endpoint = "/api/content?orderby=history.createdDate desc&expand=body.view,version,space";
            if (request.CreatedFrom.HasValue || request.UpdatedFrom.HasValue)
                endpoint += ",history,history.lastUpdated";

            var apiRequest = new ApiRequest(endpoint, Method.Get, Creds);
            AddRequestParameters(apiRequest, request, start, limit);

            var response = await Client.ExecuteWithErrorHandling<SearchContentResponse>(apiRequest);
            FilterResults(response, request);
            allResults.AddRange(response.Results);

            if (response.Size < limit) break;
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

    private void AddRequestParameters(ApiRequest apiRequest, FilterContentRequest request, int? start = null,
        int? limit = null)
    {
        if (!string.IsNullOrEmpty(request.Status))
            apiRequest.AddParameter("status", request.Status, ParameterType.QueryString);
        if (!string.IsNullOrEmpty(request.ContentType))
            apiRequest.AddParameter("type", request.ContentType, ParameterType.QueryString);
        if (start.HasValue)
            apiRequest.AddParameter("start", start.Value, ParameterType.QueryString);
        if (limit.HasValue)
            apiRequest.AddParameter("limit", limit.Value, ParameterType.QueryString);
    }

    private void FilterResults(SearchContentResponse response, FilterContentRequest request)
    {
        if (response.Results == null! || !response.Results.Any()) return;

        if (request.CreatedFrom.HasValue)
            response.Results = response.Results.Where(x =>
                x.History != null && x.History.CreatedDate.ToUniversalTime() >=
                request.CreatedFrom.Value.ToUniversalTime()).ToList();

        if (request.UpdatedFrom.HasValue)
            response.Results = response.Results.Where(x =>
                x.History != null && x.History.LastUpdated.When.ToUniversalTime() >=
                request.UpdatedFrom.Value.ToUniversalTime()).ToList();
    }
}