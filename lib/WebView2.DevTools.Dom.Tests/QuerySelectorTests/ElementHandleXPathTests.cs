using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class ElementHandleXPathTests : DevTooolsContextBaseTest
    {
        public ElementHandleXPathTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldQueryExistingElement()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/playground.html");
            await DevToolsContext.SetContentAsync("<html><body><div class=\"second\"><div class=\"inner\">A</div></div></body></html>");
            var html = await DevToolsContext.QuerySelectorAsync("html");
            var second = await html.XPathAsync("./body/div[contains(@class, 'second')]");
            var inner = await second[0].XPathAsync("./div[contains(@class, 'inner')]");
            var content = await DevToolsContext.EvaluateFunctionAsync<string>("e => e.textContent", inner[0]);
            Assert.Equal("A", content);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnNullForNonExistingElement()
        {
            await DevToolsContext.SetContentAsync("<html><body><div class=\"second\"><div class=\"inner\">B</div></div></body></html>");
            var html = await DevToolsContext.QuerySelectorAsync("html");
            var second = await html.XPathAsync("/div[contains(@class, 'third')]");
            Assert.Empty(second);
        }
    }
}
