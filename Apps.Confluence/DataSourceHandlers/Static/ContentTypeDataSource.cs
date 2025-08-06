using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Confluence.DataSourceHandlers.Static;

public class ContentTypeDataSource : IStaticDataSourceHandler
{
    public Dictionary<string, string> GetData()
    {
        return new()
        {
            { "page", "Page" },
            { "blogpost", "Blogpost" },
            { "comment", "Comment" },
            { "attachment", "Attachment" }
        };
    }
}