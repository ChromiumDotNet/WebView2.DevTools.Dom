using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit.Abstractions;
using Xunit;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{

    [Collection(TestConstants.TestFixtureCollectionName)]
    public class HtmlInputElementTests : DevTooolsContextBaseTest
    {
        public HtmlInputElementTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/checkbox.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlInputElement>("#agree");

            Assert.NotNull(element);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetChecked()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/checkbox.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlInputElement>("#agree");

            await element.SetCheckedAsync(true);

            var actual = await element.GetCheckedAsync();

            Assert.True(actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetIndeterminate()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/checkbox.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlInputElement>("#agree");

            await element.SetIndeterminateAsync(true);

            var actual = await element.GetIndeterminateAsync();

            Assert.True(actual);
        }
    }
}
