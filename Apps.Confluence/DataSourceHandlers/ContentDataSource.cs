using Apps.Confluence.Api;
using Apps.Confluence.Constants;
using Apps.Confluence.Extensions;
using Apps.Confluence.Invocables;
using Apps.Confluence.Models.Entities.Content;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Confluence.DataSourceHandlers;

public class ContentDataSource(InvocationContext invocationContext) : AppInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        string cql = CqlQueries.EditableContentCql;
        if (!string.IsNullOrWhiteSpace(context.SearchString))
            cql += $" AND title ~ {context.SearchString.ToCqlString() + "*"}";
        
        var request = new ApiRequest("/rest/api/content/search", Method.Get, Creds)
            .AddParameter("cql", cql, ParameterType.QueryString);
        
        var response = await Client.Paginate<ContentEntity>(request, maxPages: 1);
        return response.Select(x => new DataSourceItem(x.ContentId, $"{x.Title} ({x.Type})")).ToList();
    }
}