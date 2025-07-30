using Tests.Confluence.Base;

namespace Tests.Confluence
{
    [TestClass]
    public class DataHandlerTests : TestBase
    {
        [TestMethod]
        public async Task SpaceDataSource_IsSuccess()
        {
          var handler = new Apps.Confluence.DataSourceHandlers.SpaceDataSource(InvocationContext);
            var context = new Blackbird.Applications.Sdk.Common.Dynamic.DataSourceContext
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
        public async Task ContentDataSource_IsSuccess()
        {
            var handler = new Apps.Confluence.DataSourceHandlers.ContentDataSource(InvocationContext);
            var context = new Blackbird.Applications.Sdk.Common.Dynamic.DataSourceContext
            {
                SearchString = ""
            };


            var result = await handler.GetDataAsync(context, CancellationToken.None);

            foreach (var space in result)
            {
                Console.WriteLine($"Content ID: {space.Key}, Name: {space.Value}");
            }
            Assert.IsNotNull(result);
        }
    }


}
