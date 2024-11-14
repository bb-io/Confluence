using Apps.Confluence.Api;
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
        var allResults = new Dictionary<string, string>();
        var start = 0;
        var limit = 25;

        while (true)
        {
            var request = new ApiRequest("/api/space", Method.Get, Creds);
            request.AddParameter("start", start, ParameterType.QueryString);
            request.AddParameter("limit", limit, ParameterType.QueryString);

            var spacesResponse = await Client.ExecuteWithErrorHandling<SearchSpaceResponse>(request);

            if (spacesResponse.Results != null! && spacesResponse.Results.Any())
            {
                var filteredResults = spacesResponse.Results
                    .Where(x => context.SearchString == null || x.Name.Contains(context.SearchString))
                    .ToDictionary(x => x.Id, x => x.Name);

                foreach (var kvp in filteredResults)
                {
                    allResults[kvp.Key] = kvp.Value;
                }
            }

            if (spacesResponse.Size < limit)
            {
                break;
            }

            start += limit;
        }

        return allResults;
    }
}