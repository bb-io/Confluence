using Apps.Confluence.Actions;
using Apps.Confluence.Models.Identifiers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Confluence.Base;

namespace Tests.Confluence
{
    [TestClass]
    public class ContentTests :TestBase
    {

        [TestMethod]
        public async Task SearchContentAsync_ReturnsResults()
        {          
            var contentActions = new ContentActions(InvocationContext, FileManager);
            var request = new Apps.Confluence.Models.Requests.Content.FilterContentRequest
            {
                Status = "current",
                //ContentType = "page",
                //CreatedFrom = DateTime.UtcNow.AddDays(-30)
            };
            var response = await contentActions.SearchContentAsync(request);

            var json = JsonConvert.SerializeObject(response, Formatting.Indented);
            Console.WriteLine(json);

            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task GetContentAsync_ReturnsResults()
        {
            var contentActions = new ContentActions(InvocationContext, FileManager);
            var request = new ContentIdentifier
            {
                ContentId = "997982220",
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
                ContentId = "997982220",
            };
            var response = await contentActions.GetContentAsHtmlAsync(request);

            var json = JsonConvert.SerializeObject(response, Formatting.Indented);
            Console.WriteLine(json);

            Assert.IsNotNull(response);
        }
    }
}
