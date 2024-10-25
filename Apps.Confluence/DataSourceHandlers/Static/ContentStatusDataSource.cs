using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Confluence.DataSourceHandlers.Static;

public class ContentStatusDataSource : IStaticDataSourceHandler
{
    public Dictionary<string, string> GetData()
    {
        return new()
        {
            { "current", "Current" },
            { "deleted", "Deleted" },
            { "draft", "Draft" }
        };
    }
}