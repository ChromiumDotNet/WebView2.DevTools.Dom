using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit.Abstractions;
using Xunit;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class AsyncExtensionTests : DevTooolsContextBaseTest
    {
        public AsyncExtensionTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldAndThenFunction()
        {
            const string expected = "checkbox";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/checkbox.html");

            var element = DevToolsContext.QuerySelectorAsync<HtmlInputElement>("#agree");
            var actual = await element.AndThen(x => x.GetAttributeAsync<string>("type"));

            Assert.Equal(expected, actual);
            Assert.True(element.Result.IsDisposed);
        }

        [WebView2ContextFact]
        public async Task ShouldAndThenFunctionChain()
        {
            const string expected = "checkbox";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/checkbox.html");

            var element = DevToolsContext.QuerySelectorAsync("body");
            var actual = await element
                .AndThen(x => x.QuerySelectorAsync("#agree"))
                .AndThen(x => x.GetAttributeAsync<string>("type"));

            Assert.Equal(expected, actual);
            Assert.True(element.Result.IsDisposed);
        }

        [WebView2ContextFact]
        public async Task ShouldAndThenFunctionWithoutDispose()
        {
            const string expected = "checkbox";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/checkbox.html");

            var element = DevToolsContext.QuerySelectorAsync<HtmlInputElement>("#agree");
            var actual = await element.AndThen(x => x.GetAttributeAsync<string>("type"), dispose: false);

            Assert.Equal(expected, actual);
            Assert.False(element.Result.IsDisposed);
        }
    }
}
