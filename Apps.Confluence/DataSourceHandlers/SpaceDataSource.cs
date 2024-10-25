﻿using Apps.Confluence.Api;
using Apps.Confluence.Invocables;
using Apps.Confluence.Models.Responses.Spaces;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Confluence.DataSourceHandlers;

public class SpaceDataSource(InvocationContext invocationContext) : AppInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var request = new ApiRequest("/api/space", Method.Get, Creds);
        var spacesResponse = await Client.ExecuteWithErrorHandling<SearchSpaceResponse>(request);
        
        return spacesResponse.Results
            .Where(x => context.SearchString == null || x.Name.Contains(context.SearchString))
            .ToDictionary(x => x.Id, x => x.Name);
    }
}