using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.QuerySelectorTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class PageXPathTests : DevTooolsContextBaseTest
    {
        public PageXPathTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldQueryExistingElement()
        {
            await DevToolsContext.SetContentAsync("<section>test</section>");
            var elements = await DevToolsContext.XPathAsync("/html/body/section");
            Assert.NotNull(elements[0]);
            Assert.Single(elements);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnEmptyArrayForNonExistingElement()
        {
            var elements = await DevToolsContext.XPathAsync("/html/body/non-existing-element");
            Assert.Empty(elements);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnMultipleElements()
        {
            await DevToolsContext.SetContentAsync("<div></div><div></div>");
            var elements = await DevToolsContext.XPathAsync("/html/body/div");
            Assert.Equal(2, elements.Length);
        }
    }
}
