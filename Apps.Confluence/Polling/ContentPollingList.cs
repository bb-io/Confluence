using Apps.Confluence.Api;
using Apps.Confluence.Constants;
using Apps.Confluence.Extensions;
using Apps.Confluence.Invocables;
using Apps.Confluence.Models.Entities.Content;
using Apps.Confluence.Models.Responses.Content;
using Apps.Confluence.Polling.Models;
using Apps.Confluence.Polling.Models.Requests;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.Sdk.Common;
using RestSharp;

namespace Apps.Confluence.Polling;

[PollingEventList]
public class ContentPollingList(InvocationContext invocationContext) : AppInvocable(invocationContext)
{
    [BlueprintEventDefinition(BlueprintEvent.ContentCreatedOrUpdated)]
    [MultipleEvents, PollingEvent("On content created", "Triggered when content is created or existing content is updated.")]
    public async Task<PollingEventResponse<DateMemory, List<ContentResponse>>> OnContentCreated(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] FilterContentPollingRequest input)
    {
        var pollingStart = DateTime.UtcNow;

        if (request.Memory is null)
            return new() { FlyBird = false, Memory = new() { LastInteractionDate = pollingStart } };
        
        var content = await FetchContent(request.Memory.LastInteractionDate.ToUniversalTime(), input);
        return new()
        {
            FlyBird = content.Count != 0,
            Result = content.Select(x => new ContentResponse(x)).ToList(),
            Memory = new() { LastInteractionDate = pollingStart }
        };
    }

    [MultipleEvents, PollingEvent("On content updated", "Triggered when existing content is updated.")]
    public async Task<PollingEventResponse<DateMemory, List<ContentResponse>>> OnContentUpdated(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] FilterContentPollingRequest input)
    {
        var pollingStart = DateTime.UtcNow;

        if (request.Memory is null)
            return new() { FlyBird = false, Memory = new() { LastInteractionDate = pollingStart } };
        
        var from = request.Memory.LastInteractionDate.ToUniversalTime();
        
        var content = await FetchContent(from, input);
        var filtered = content.Where(x => x.History?.CreatedDate.ToUniversalTime() < from).ToList();
        
        return new()
        {
            FlyBird = filtered.Count != 0,
            Result = filtered.Select(x => new ContentResponse(x)).ToList(),
            Memory = new() { LastInteractionDate = pollingStart }
        };
    }

    private async Task<List<ContentEntity>> FetchContent(DateTime from, FilterContentPollingRequest input)
    {
        var minutesAgo = (int)Math.Ceiling((DateTime.UtcNow - from).TotalMinutes) + 1;

        var parts = new List<string>
        {
            $"lastmodified >= now(\"-{minutesAgo}m\")",
            string.IsNullOrEmpty(input.ContentType) ? CqlQueries.EditableContentCql : $"type = {input.ContentType.ToLower()}"
        };

        if (!string.IsNullOrEmpty(input.SpaceId))
            parts.Add($"space = \"{await Client.GetSpaceKey(Creds, input.SpaceId)}\"");

        var apiRequest = new ApiRequest("/rest/api/content/search", Method.Get, Creds)
            .AddParameter("cql", string.Join(" AND ", parts), ParameterType.QueryString)
            .AddParameter("expand", "version,space,history,history.lastUpdated", ParameterType.QueryString);

        var response = await Client.Paginate<ContentEntity>(apiRequest);
        var content = response
            .Where(x => x.History?.LastUpdated.When.ToUniversalTime() >= from)
            .Where(x => string.IsNullOrEmpty(input.Status) || x.Status == input.Status)
            .ToList();
        
        return content;
    }
}