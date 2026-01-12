using Blackbird.Applications.Sdk.Common;

namespace Apps.Confluence.Models.Responses.Properties;

public class ContentPropertyListResponse
{
    public int Size { get; set; }
    public List<ContentPropertyResponse> Results { get; set; }

    [DefinitionIgnore]
    public ConfluenceLinks Links { get; set; }
}

public class ConfluenceLinks
{
    public string Self { get; set; }
    public string Next { get; set; }
}

