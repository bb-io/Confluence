using Apps.Confluence.Models.Responses.Content;
using HtmlAgilityPack;

namespace Apps.Confluence.Utils;

public static class HtmlConverter
{
    public static string ConvertToHtml(ContentResponse content)
    {
        var htmlDoc = new HtmlDocument();

        var htmlNode = HtmlNode.CreateNode("<html></html>");
        htmlDoc.DocumentNode.AppendChild(htmlNode);

        var headNode = HtmlNode.CreateNode("<head></head>");
        
        var titleNode = HtmlNode.CreateNode($"<title>{content.Title}</title>");
        headNode.AppendChild(titleNode);
        
        var metaNode = HtmlNode.CreateNode($"<meta name=\"blackbird-content-id\" content=\"{content.Id}\" />");
        headNode.AppendChild(metaNode);
        
        htmlNode.AppendChild(headNode);

        var bodyNode = HtmlNode.CreateNode("<body></body>");
        bodyNode.InnerHtml = content.Body.Storage.Value;
        htmlNode.AppendChild(bodyNode);

        return htmlDoc.DocumentNode.OuterHtml;
    }
    
    public static HtmlContentEntity ExtractHtmlContent(string html)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var title = htmlDoc.DocumentNode.SelectSingleNode("//title")?.InnerText
            ?? throw new Exception("Title not found in the HTML content.");
        var content = htmlDoc.DocumentNode.SelectSingleNode("//body")?.InnerHtml
            ?? throw new Exception("Body not found in the HTML content.");
        var idNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='blackbird-content-id']");
        var id = idNode?.GetAttributeValue("content", string.Empty);

        return new HtmlContentEntity(id, title, content);
    }
}

public record HtmlContentEntity(string? Id, string Title, string HtmlContent);