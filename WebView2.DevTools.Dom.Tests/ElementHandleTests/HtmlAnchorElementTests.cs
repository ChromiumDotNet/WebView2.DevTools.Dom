using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit.Abstractions;
using Xunit;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{

    [Collection(TestConstants.TestFixtureCollectionName)]
    public class HtmlAnchorElementTests : DevTooolsContextBaseTest
    {
        public HtmlAnchorElementTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlAnchorElement>("a");

            Assert.NotNull(element);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetDisabled()
        {
            const string expected = "_blank";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlAnchorElement>("a");

            await element.SetTargetAsync(expected);

            var actual = await element.GetTargetAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetHref()
        {
            const string expected = "https://microsoft.com/";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlAnchorElement>("a");

            await element.SetHrefAsync(expected);

            var actual = await element.GetHrefAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetType()
        {
            const string expected = "text/html";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlAnchorElement>("a");

            await element.SetTypeAsync(expected);

            var actual = await element.GetTypeAsync();

            Assert.Equal(expected, actual);
        }
    }
}
