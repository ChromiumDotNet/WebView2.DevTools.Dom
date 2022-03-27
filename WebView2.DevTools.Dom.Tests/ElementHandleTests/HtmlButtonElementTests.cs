using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit.Abstractions;
using Xunit;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{

    [Collection(TestConstants.TestFixtureCollectionName)]
    public class HtmlButtonElementTests : DevTooolsContextBaseTest
    {
        public HtmlButtonElementTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("button");

            Assert.NotNull(button);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetDisabled()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("button");

            await button.SetDisabledAsync(true);

            var actual = await button.GetDisabledAsync();

            Assert.True(actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetName()
        {
            const string expected = "buttonName";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("button");

            await button.SetNameAsync(expected);

            var actual = await button.GetNameAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetType()
        {
            const HtmlButtonElementType expected = HtmlButtonElementType.Submit;

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("button");

            await button.SetTypeAsync(expected);

            var actual = await button.GetTypeAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetValue()
        {
            const string expected = "Test Button";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("button");

            await button.SetValueAsync(expected);

            var actual = await button.GetValueAsync();

            Assert.Equal(expected, actual);
        }
    }
}
