using Apps.Confluence.Polling;
using Apps.Confluence.Polling.Models;
using Blackbird.Applications.Sdk.Common.Polling;
using Tests.Confluence.Base;

namespace Tests.Confluence
{
    [TestClass]
    public class PollingTests : TestBase
    {
        [TestMethod]
        public async Task OnContentUpdated_ReturnsResults()
        {
            var polling = new ContentPollingList(InvocationContext);
            var request = new PollingEventRequest<DateMemory>
            {
                Memory = new DateMemory
                {
                    LastInteractionDate = DateTime.UtcNow.AddDays(-10)
                }
            };
            var filterRequest = new Apps.Confluence.Polling.Models.Requests.FilterContentPollingRequest
            {
                ContentType = "page",
            };
            var response = await polling.OnContentUpdated(request, filterRequest);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
            Console.WriteLine(json);
            Assert.IsNotNull(response);
        }
    }
}
