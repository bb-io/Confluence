using Apps.Confluence.Constants;
using Apps.Confluence.DataSourceHandlers;
using Apps.Confluence.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Confluence.Models.Requests.Content;

public class FilterContentRequest
{
    [Display("Content type"), StaticDataSource(typeof(ContentTypeDataSource))]
    public string? ContentType { get; set; }

    [Display("Status"), StaticDataSource(typeof(ContentStatusDataSource))]
    public string? Status { get; set; }

    [Display("Space ID"), DataSource(typeof(SpaceDataSource))]
    public string? SpaceId { get; set; } = string.Empty;

    [Display("Created from")]
    public DateTime? CreatedFrom { get; set; }
    
    [Display("Updated from")]
    public DateTime? UpdatedFrom { get; set; }

    [Display("Parent ID")]
    public string? ParentId { get; set; }

    [Display("CQL query")]
    public string? CqlQuery { get; set; }

    public void Validate()
    {
        if (!string.IsNullOrEmpty(ContentType) && !ContentTypes.ValidContentTypes.Contains(ContentType.ToLower()))
        {
            throw new PluginMisconfigurationException(
                $"Invalid content type: {ContentType}. Valid types are: {string.Join(", ", ContentTypes.ValidContentTypes)}");
        }
        
        if (CreatedFrom.HasValue && CreatedFrom.Value.ToUniversalTime() > DateTime.UtcNow)
            throw new PluginMisconfigurationException("Created from date cannot be in the future.");
        
        if (UpdatedFrom.HasValue && UpdatedFrom.Value.ToUniversalTime() > DateTime.UtcNow)
            throw new PluginMisconfigurationException("Updated from date cannot be in the future.");
        
        if (!string.IsNullOrEmpty(CqlQuery))
        {
            string trimmed = CqlQuery.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                throw new PluginMisconfigurationException("CQL query cannot be empty or whitespace.");

            if (trimmed.Contains(';') || trimmed.Contains("--"))
                throw new PluginMisconfigurationException("CQL query contains invalid characters (e.g., ';' or '--').");
        }
    }
}