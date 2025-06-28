using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class IsIntersectingViewportTests : DevTooolsContextBaseTest
    {
        public IsIntersectingViewportTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/offscreenbuttons.html");
            for (var i = 0; i < 11; ++i)
            {
                var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("#btn" + i);
                // All but last button are visible.
                var visible = i < 10;
                Assert.Equal(visible, await button.IsIntersectingViewportAsync());
            }
        }
    }
}
