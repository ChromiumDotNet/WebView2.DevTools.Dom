using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using WebView2.DevTools.Dom.Tests.Attributes;

namespace WebView2.DevTools.Dom.Tests.QuerySelectorTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class DevToolsContextQuerySelectorAllTests : DevTooolsContextBaseTest
    {
        public DevToolsContextQuerySelectorAllTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var elements = await DevToolsContext.QuerySelectorAllAsync<HtmlTableRowElement>("tr");

            Assert.NotNull(elements);
            Assert.Equal(4, elements.Length);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnEmptyArray()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var elements = await DevToolsContext.QuerySelectorAllAsync<HtmlTableRowElement>("#table2 tr");

            Assert.NotNull(elements);
            Assert.Empty(elements);
        }

        [WebView2ContextFact]
        public async Task ShouldQueryExistingElements()
        {
            await DevToolsContext.SetContentAsync("<div>A</div><br/><div>B</div>");
            var elements = await DevToolsContext.QuerySelectorAllAsync("div");
            Assert.Equal(2, elements.Length);
            var tasks = elements.Select(element => DevToolsContext.EvaluateFunctionAsync<string>("e => e.textContent", element));
            Assert.Equal(new[] { "A", "B" }, await Task.WhenAll(tasks));
        }

        [WebView2ContextFact]
        public async Task ShouldReturnEmptyArrayIfNothingIsFound()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var elements = await DevToolsContext.QuerySelectorAllAsync("div");
            Assert.Empty(elements);
        }
    }
}
