using System.Linq;
using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class ElementHandleQuerySelectorAllTests : DevTooolsContextBaseTest
    {
        public ElementHandleQuerySelectorAllTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldQueryExistingElements()
        {
            await DevToolsContext.SetContentAsync("<html><body><div>A</div><br/><div>B</div></body></html>");
            var html = await DevToolsContext.QuerySelectorAsync("html");
            var elements = await html.QuerySelectorAllAsync("div");
            Assert.Equal(2, elements.Length);
            var tasks = elements.Select(element => DevToolsContext.EvaluateFunctionAsync<string>("e => e.textContent", element));
            Assert.Equal(new[] { "A", "B" }, await Task.WhenAll(tasks));
        }

        [WebView2ContextFact]
        public async Task ShouldReturnEmptyArrayForNonExistingElements()
        {
            await DevToolsContext.SetContentAsync("<html><body><span>A</span><br/><span>B</span></body></html>");
            var html = await DevToolsContext.QuerySelectorAsync("html");
            var elements = await html.QuerySelectorAllAsync("div");
            Assert.Empty(elements);
        }
    }
}
