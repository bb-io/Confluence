namespace Apps.Confluence.Models.Entities.Content;

public class ContentHistory
{
    public DateTime CreatedDate { get; set; }

    public ContentLastUpdated LastUpdated { get; set; } = new();
}