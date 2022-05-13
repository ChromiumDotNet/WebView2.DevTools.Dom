using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class HoverTests : DevTooolsContextBaseTest
    {
        public HoverTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/scrollable.html");
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("#button-6");
            await button.HoverAsync();
            Assert.Equal("button-6", await DevToolsContext.EvaluateExpressionAsync<string>(
                "document.querySelector('button:hover').id"));
        }
    }
}
