using Apps.Confluence.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.Confluence.Base;

namespace Tests.Confluence;

[TestClass]
public class DataHandlerTests : TestBase
{
    [TestMethod]
    public async Task SpaceDataSource_IsSuccess()
    {
        var handler = new SpaceDataSource(InvocationContext);
        var context = new DataSourceContext
        {
            SearchString = ""
        };


        var result = await handler.GetDataAsync(context, CancellationToken.None);

        foreach (var space in result)
        {
            Console.WriteLine($"Space ID: {space.Key}, Name: {space.Value}");
        }
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ContentDataSource_ReturnsContent()
    {
        // Arrange
        var handler = new ContentDataSource(InvocationContext);
        var context = new DataSourceContext { SearchString = "" };

        // Act
        var result = await handler.GetDataAsync(context, CancellationToken.None);
        
        // Assert
        PrintDataHandlerResult(result);
    }
}