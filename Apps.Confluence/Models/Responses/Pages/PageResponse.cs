namespace Apps.Confluence.Models.Responses.Pages;

public class PageResponse
{
    public string Id { get; set; }
    public string Title { get; set; }
    public PageBody Body { get; set; }
}

public class PageBody
{
    public PageBodyStorage Storage { get; set; }
}

public class PageBodyStorage
{
    public string Value { get; set; }
}
