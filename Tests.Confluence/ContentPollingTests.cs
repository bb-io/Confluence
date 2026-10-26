using Apps.Confluence.Polling;
using Apps.Confluence.Polling.Models;
using Apps.Confluence.Polling.Models.Requests;
using Blackbird.Applications.Sdk.Common.Polling;
using Tests.Confluence.Base;

namespace Tests.Confluence;

[TestClass]
public class ContentPollingTests : TestBase
{
    private ContentPollingList Polling => new(InvocationContext);

    [TestMethod]
    public async Task OnContentCreated_ReturnsCreatedContent()
    {
        // Arrange
        var memory = new DateMemory { LastInteractionDate = DateTime.UtcNow - TimeSpan.FromHours(10) };
        var pollingRequest = new PollingEventRequest<DateMemory> { Memory = memory };
        var filter = new FilterContentPollingRequest 
        { 
            ContentType = "page"
        };

        // Act
        var result = await Polling.OnContentCreated(pollingRequest, filter);

        // Assert
        PrintJsonResult(result.Result ?? []);
        Assert.IsNotNull(result);
    }
    
    [TestMethod]
    public async Task OnContentUpdated_ReturnsUpdatedContent()
    {
        // Arrange
        var memory = new DateMemory { LastInteractionDate = DateTime.UtcNow - TimeSpan.FromMinutes(10) };
        var pollingRequest = new PollingEventRequest<DateMemory> { Memory = memory };
        var filter = new FilterContentPollingRequest 
        { 
            ContentType = "page"
        };

        // Act
        var result = await Polling.OnContentUpdated(pollingRequest, filter);

        // Assert
        PrintJsonResult(result.Result ?? []);
        Assert.IsNotNull(result);
    }
}
