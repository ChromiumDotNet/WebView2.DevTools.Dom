using System;
using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class ElementHandleQuerySelectorTests : DevTooolsContextBaseTest
    {
        public ElementHandleQuerySelectorTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldQueryExistingElement()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/playground.html");
            await DevToolsContext.SetContentAsync("<html><body><div class=\"second\"><div class=\"inner\">A</div></div></body></html>");
            var html = await DevToolsContext.QuerySelectorAsync("html");
            var second = await html.QuerySelectorAsync(".second");
            var inner = await second.QuerySelectorAsync(".inner");
            var content = await DevToolsContext.EvaluateFunctionAsync<string>("e => e.textContent", inner);
            Assert.Equal("A", content);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnNullForNonExistingElement()
        {
            await DevToolsContext.SetContentAsync("<html><body><div class=\"second\"><div class=\"inner\">B</div></div></body></html>");
            var html = await DevToolsContext.QuerySelectorAsync("html");
            var second = await html.QuerySelectorAsync(".third");
            Assert.Null(second);
        }
    }
}
