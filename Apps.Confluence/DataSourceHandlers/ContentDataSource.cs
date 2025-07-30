using Apps.Confluence.Actions;
using Apps.Confluence.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Confluence.DataSourceHandlers;

public class ContentDataSource(InvocationContext invocationContext) : AppInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var actions = new ContentActions(InvocationContext, null!);
        var campaigns = await actions.SearchContentAsync(new());
        
        return campaigns.Results
            .Where(x => context.SearchString == null || x.Title.Contains(context.SearchString))
            .ToDictionary(x => x.Id, v => $"{ v.Title} ({v.Type})");
    }
}