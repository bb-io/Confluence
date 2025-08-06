using Blackbird.Applications.Sdk.Common;

namespace Apps.Confluence.Models.Responses;

public class BasePaginationResponse<T>
{
    public virtual List<T> Results { get; set; } = new();

    [DefinitionIgnore]
    public int Start { get; set; }

    [DefinitionIgnore]
    public int Limit { get; set; }
    
    [DefinitionIgnore]
    public int? Size { get; set; }
}