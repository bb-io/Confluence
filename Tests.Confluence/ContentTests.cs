using Apps.Confluence.Actions;
using Apps.Confluence.Models.Identifiers;
using Apps.Confluence.Models.Requests.Content;
using Newtonsoft.Json;
using Tests.Confluence.Base;

namespace Tests.Confluence;

[TestClass]
public class ContentTests :TestBase
{
    [TestMethod]
    public async Task SearchContentAsync_ReturnsResults()
    {
        // Arrange
        var contentActions = new ContentActions(InvocationContext, FileManager);
        var request = new FilterContentRequest
        {
            //SpaceId = "163844"
        };
        
        // Act
        var response = await contentActions.SearchContentAsync(request);

        // Assert
        PrintJsonResult(response);
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task GetContentAsync_ReturnsResults()
    {
        var contentActions = new ContentActions(InvocationContext, FileManager);
        var request = new ContentIdentifier
        {
            ContentId = "2296119692",
        };
        var response = await contentActions.GetContentAsync(request);

        var json = JsonConvert.SerializeObject(response, Formatting.Indented);
        Console.WriteLine(json);

        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task GetContentHTMLAsync_ReturnsResults()
    {
        var contentActions = new ContentActions(InvocationContext, FileManager);
        var request = new ContentIdentifier
        {
            ContentId = "98406",
        };
        var response = await contentActions.GetContentAsHtmlAsync(request);

        var json = JsonConvert.SerializeObject(response, Formatting.Indented);
        Console.WriteLine(json);

        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task DeleteContent_ReturnsResults()
    {
        var contentActions = new ContentActions(InvocationContext, FileManager);
        var request = new ContentIdentifier
        {
            ContentId = "1441825",
        };
        await contentActions.DeleteContentAsync(request);

        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task CreateContent_ReturnsResults()
    {
        var contentActions = new ContentActions(InvocationContext, FileManager);
        var request = new Apps.Confluence.Models.Requests.Content.CreateContentRequest
        {
            Type = "blogpost",
            Title = "Test Page 3",
            SpaceId = "65882",
            Status = "current",
            Body = "<p>This is a test page created via API.</p>"
        };
        var response = await contentActions.CreateContentAsync(request);

        var json = JsonConvert.SerializeObject(response, Formatting.Indented);
        Console.WriteLine(json);

        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task CreateContentFromHtml_ReturnsResults()
    {
        var contentActions = new ContentActions(InvocationContext, FileManager);
        var request = new Apps.Confluence.Models.Requests.Content.CreateContentFromHtmlRequest
        {
            File=new Blackbird.Applications.Sdk.Common.Files.FileReference { Name= "content-98411.html" },
            SpaceId = "65882",
            ContentType = "blogpost"
            //1605647
        };
        var response = await contentActions.CreateContentFromHtmlAsync(request);

        var json = JsonConvert.SerializeObject(response, Formatting.Indented);
        Console.WriteLine(json);

        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task UpdateContentFromHtml_ReturnsResults()
    {
        var contentActions = new ContentActions(InvocationContext, FileManager);
        var request = new Apps.Confluence.Models.Requests.Content.UpdateContentRequest
        {
            File = new Blackbird.Applications.Sdk.Common.Files.FileReference { Name = "content-98411.html" },
            ContentId = "1048591"
        };
        await contentActions.UpdateContentFromHtml(request);

        Assert.IsTrue(true);
    }
}
