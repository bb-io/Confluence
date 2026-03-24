using Apps.Confluence.Polling;
using Apps.Confluence.Polling.Models;
using Apps.Confluence.Polling.Models.Requests;
using Blackbird.Applications.Sdk.Common.Polling;
using Tests.Confluence.Base;

namespace Tests.Confluence;

[TestClass]
public class ContentPollingTests : TestBase
{
    private readonly ContentPollingList _polling;

    public ContentPollingTests() => _polling = new ContentPollingList(InvocationContext);

    [TestMethod]
    public async Task OnContentCreated_ReturnsCreatedContent()
    {
        // Arrange
        var memory = new DateMemory { LastInteractionDate = DateTime.UtcNow - TimeSpan.FromDays(1) };
        var pollingRequest = new PollingEventRequest<DateMemory> { Memory = memory };
        var filter = new FilterContentPollingRequest 
        { 
            ContentType = "page"
        };

        // Act
        var result = await _polling.OnContentCreated(pollingRequest, filter);

        // Assert
        PrintJsonResult(result);
        Assert.IsNotNull(result);
    }
}
